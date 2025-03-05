using Caliburn.Micro;
using WLVPN.Factories;
using WLVPN.Helpers;
using WLVPN.Input;
using WLVPN.Interfaces;
using WLVPN.Utils;
using WLVPN.ViewModels;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;

using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Controls;
using System.Windows.Input;
using VpnSDK;
using VpnSDK.Interfaces;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Providers;
using Container = SimpleInjector.Container;
using WLVPN.ViewModels.Information;
using System.Configuration;

namespace WLVPN
{
    public class AppBootstrapper : BootstrapperBase
    {
        private ISDK _sdk;
        public static readonly Container ContainerInstance = new Container();
        public static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        public static IObservable<LogEvent> LogObservable { get; internal set; }

        public static ReplaySubject<LogEvent> LogSubject { get; internal set; } = new ReplaySubject<LogEvent>();

        public static string LogFileFolder { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AssemblyName, "Diagnostics");


        public AppBootstrapper()
        {
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Verbose()
#else
                .MinimumLevel.Information()
#endif
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithDemystifiedStackTraces()
                 .WriteTo.Observers(e => e.Subscribe(LogSubject.OnNext))
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}")
                .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(x => x.Exception != null || x.Level == LogEventLevel.Error || x.Level == LogEventLevel.Fatal || x.Level == LogEventLevel.Warning).Enrich.WithProcessId().WriteTo.File(new JsonFormatter(), Path.Combine(LogFileFolder, "error.json"), rollOnFileSizeLimit: true, fileSizeLimitBytes: 1048576 * 5))
                .WriteTo.File(Path.Combine(LogFileFolder, "diagnostics.txt"), outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}"
#if !DEBUG
                    , rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 5,
                    fileSizeLimitBytes: 2097152
#endif
                    )
                .CreateLogger();

            Log.Information($"=== {AssemblyName} v{Assembly.GetExecutingAssembly().GetName().Version} {DateTime.Now:yyyy-MM-dd HH:mm:sszzz} ===");
            Log.Information($"Windows Build Number: {DiagnosticsHelper.GetOperatingSystemVersion()}");

            // Subscribe to UnhandledException event
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Fixing settings if they're broken.
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            }
            catch (ConfigurationErrorsException ex)
            {
                if (ex.Filename != null && File.Exists(ex.Filename))
                {
                    File.Delete(ex.Filename);
                    Log.Information("Settings file was removed.");
                }
            }

            // Upgrade settings.
            if (Properties.Settings.Default.CallSettingsUpgrade)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.CallSettingsUpgrade = false;
                Properties.Settings.Default.Save();
            }

            // Log user's settings
            Log.Information(DiagnosticsHelper.GetSettingsStateInfo());            

            // Localization setup
            LocalizeDictionary.Instance.IncludeInvariantCulture = false;
            ResxLocalizationProvider.Instance.UpdateCultureList(AssemblyName, "Strings");
            if (string.IsNullOrEmpty(Properties.Settings.Default.Culture))
            {
                if (LocalizeDictionary.Instance.MergedAvailableCultures.Any(x => x.TwoLetterISOLanguageName == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName))
                {
                    Properties.Settings.Default.Culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Properties.Settings.Default.Culture = "en";
                    Properties.Settings.Default.Save();
                }
            }
            LocalizeDictionary.Instance.Culture = new CultureInfo(Properties.Settings.Default.Culture);

            Initialize();

            // Allow PasswordBox to bind to Caliburn.Micro correctly.
            ConventionManager.AddElementConvention<PasswordBox>(
                PasswordBoxHelper.BoundPasswordProperty,
                "Password",
                "PasswordChanged");
        }

        private void CurrentDomainOnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            Log.Debug(e.Exception?.Demystify(), "First Chance Exception");
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            exception = exception?.Demystify();

            if (exception == null)
            {
                return;
            }

            try
            {
                Log.Information("Unhandled Exception, trying to dispose of SDK");
                if (!_sdk.IsDisposed)
                {
                    _sdk.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception while Disposing of SDK");
            }

            if (Debugger.IsAttached)
            {
                throw exception;
            }

            Log.Information(DiagnosticsHelper.GetSettingsStateInfo());
            Log.Fatal(exception, "Unhandled exception.");
            Log.Information($"Runtime terminating: {e.IsTerminating}");
        }

        protected override void Configure()
        {
            CreateTrigger();

            ContainerInstance.RegisterSingleton<SAPIWrapper>();
            ContainerInstance.RegisterSingleton<IWindowManager, WindowManager>();
            ContainerInstance.RegisterSingleton<IEventAggregator, EventAggregator>();

            ContainerInstance.RegisterSingleton<ShellViewModel>();
            ContainerInstance.RegisterSingleton<LoginViewModel>();
            ContainerInstance.RegisterSingleton<MainViewModel>();
            ContainerInstance.RegisterSingleton<IDialogManager, DialogConductorViewModel>();
            ContainerInstance.RegisterSingleton<IBusyManager, BusyViewModel>();
            ContainerInstance.RegisterInstance<IMessageBoxFactory>(new MessageBoxFactory());

            ContainerInstance.Register<IWifiService, WifiService>(Lifestyle.Singleton);
            ContainerInstance.Collection.Append<IMainScreenTabItem, HomeViewModel>(Lifestyle.Singleton);
            ContainerInstance.Collection.Append<IMainScreenTabItem, SettingsContainerViewModel>(Lifestyle.Singleton);
            ContainerInstance.Collection.Append<IMainScreenTabItem, InformationContainerViewModel>(Lifestyle.Singleton);

            ContainerInstance.Collection.Append<ISettingsTabItem, GeneralSettingsViewModel>(Lifestyle.Singleton);
            ContainerInstance.Collection.Append<ISettingsTabItem, ConnectionSettingsViewModel>(Lifestyle.Singleton);
            ContainerInstance.Collection.Append<ISettingsTabItem, SplitTunnelingContainerViewModel>(Lifestyle.Singleton);

            ContainerInstance.Collection.Append<IInformationTabItem, HelpViewModel>(Lifestyle.Singleton);
            ContainerInstance.Collection.Append<IInformationTabItem, LicenseViewModel>(Lifestyle.Singleton);
            ContainerInstance.Collection.Append<IInformationTabItem, DiagnosticsViewModel>(Lifestyle.Singleton);

            ContainerInstance.Collection.Append<ISplitTunnelingTabItem, SplitTunnelingApplicationViewModel>(Lifestyle.Singleton);
            ContainerInstance.Collection.Append<ISplitTunnelingTabItem, SplitTunnelingDomainViewModel>(Lifestyle.Singleton);

            /*
             The values for the ApiKey and AuthenticationToken for this app are stored in Resources\Branding.System.xaml.
             You must replace those placeholder values with a real key and authorization token
             To obtain these values you need to be a registered WLVPN reseller.
             If you have not done so already, please visit https://wlvpn.com/#contact to get started.
             */

            ContainerInstance.Register(() => _sdk = new SDKBuilder<ISDK>()
                            .SetApiKey(Resource.Get<string>("ApiKey"))
                            .SetAuthenticationToken(Resource.Get<string>("AuthenticationToken"))
                            .SetApplicationName(Resource.Get<string>("ApplicationName"))
                            .SetAutomaticRefreshTokenHandling(true)
                            .SetOpenVpnConfiguration(new VpnSDK.DTO.OpenVpnConfiguration() 
                            {
                                PreferredTapAdapter = VpnSDK.Private.OpenVpn.Enums.OpenVpnTapAdapter.TapWLVPN,
                                TapDeviceFriendlyName = Properties.Settings.Default.TapDeviceDescription 
                            })
                            .SetServerListCache(TimeSpan.FromDays(1))
                            .Create(), Lifestyle.Singleton);
        }

        private void CreateTrigger()
        {
            var defaultCreateTrigger = Parser.CreateTrigger;

            Parser.CreateTrigger = (target, triggerText) =>
            {
                if (triggerText == null)
                {
                    return defaultCreateTrigger(target, null);
                }

                var triggerDetail = triggerText
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty);

                var splits = triggerDetail.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                switch (splits[0])
                {
                    case "Key":
                        var key = (Key)Enum.Parse(typeof(Key), splits[1], true);
                        return new KeyTrigger { Key = key };

                    case "Gesture":
                        var mkg = (MultiKeyGesture)(new MultiKeyGestureConverter()).ConvertFrom(splits[1]);
                        return new KeyTrigger { Modifiers = mkg.KeySequences[0].Modifiers, Key = mkg.KeySequences[0].Keys[0] };
                }

                return defaultCreateTrigger(target, triggerText);
            };
        }

        protected override object GetInstance(Type service, string key)
        {
            if (service == null && !string.IsNullOrWhiteSpace(key))
            {
                service = Type.GetType(key);
            }
            return ContainerInstance.GetInstance(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            // Allow empty instance retrieval with SimpleInjector.
            System.IServiceProvider provider = ContainerInstance;
            Type collectionType = typeof(IEnumerable<>).MakeGenericType(service);
            IEnumerable<object> services = (IEnumerable<object>)provider.GetService(collectionType);
            return services ?? Enumerable.Empty<object>();
        }

        protected override void BuildUp(object instance)
        {
            base.BuildUp(instance);
        }


        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            ContainerInstance.Verify();

            DisplayRootViewFor<ShellViewModel>();
#if DEBUG
            if (!Execute.InDesignMode)
            {
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
            }
#endif
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            if (!_sdk.IsDisposed)
            {
                _sdk.Dispose();
            }

            Log.CloseAndFlush();

            base.OnExit(sender, e);
        }
    }
}