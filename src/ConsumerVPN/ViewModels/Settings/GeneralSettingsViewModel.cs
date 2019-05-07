using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Caliburn.Micro;
using WLVPN.Extensions;
using WLVPN.Interfaces;
using Microsoft.Win32.TaskScheduler;
using Serilog;
using Task = System.Threading.Tasks.Task;
using System.Windows;
using WLVPN.Helpers;
using WLVPN.Utils;

namespace WLVPN.ViewModels
{
    public class GeneralSettingsViewModel : Screen, ISettingsTabItem
    {
        private string TaskName = AppBootstrapper.AssemblyName;
        private bool _startAppOnWindowsStartup;

        public static Style Icon => Resource.Get<Style>("GeneralSettingsIcon");
        public SAPIWrapper Voice { get; set; }

        public string TabHeaderTitle => Properties.Strings.TabSettingsGeneral;

        public IDialogManager Dialog { get; }

        public bool StartAppOnWindowsStartup
        {
            get => _startAppOnWindowsStartup;
            set
            {
                if (_startAppOnWindowsStartup == value)
                {
                    return;
                }
                _startAppOnWindowsStartup = value;
                AddStartupTask(_startAppOnWindowsStartup);
            }
        }

        public string SelectedCulture
        {
            get => Properties.Settings.Default.Culture;
            set
            {
                string origValue = Properties.Settings.Default.Culture;
                Properties.Settings.Default.Culture = value;
                Dialog.ShowMessageBox(Properties.Strings.LanguageChangeInfo, Properties.Strings.Information, 
                    Enums.MessageBoxOptions.YesNo, d => {
                        if (d.WasSelected(Enums.MessageBoxOptions.Yes))
                        {
                            Properties.Settings.Default.Culture = value;
                            NotifyOfPropertyChange(nameof(SelectedCulture));
                        }
                        else
                        {
                            Properties.Settings.Default.Culture = origValue;
                            NotifyOfPropertyChange(nameof(SelectedCulture));
                        }
                    });
            }
        }

        public GeneralSettingsViewModel(IDialogManager dialogManager, SAPIWrapper voice)
        {
            Dialog = dialogManager;
            Voice = voice;

            Task.Run(() =>
            {
                using (TaskService tastService = new TaskService())
                {
                    try
                    {
                        Microsoft.Win32.TaskScheduler.Task serviceTask =
                            tastService.RootFolder.Tasks.FirstOrDefault(x => x.Name == TaskName);

                        if (serviceTask != null && serviceTask.Enabled)
                        {
                            _startAppOnWindowsStartup = true;
                            NotifyOfPropertyChange(nameof(StartAppOnWindowsStartup));
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Could not detect the current state of the Startup Task.");
                    }
                }
            });
        }

        private void AddStartupTask(bool startOnStartup)
        {
            Task.Run(() =>
            {
                try
                {
                    using (TaskService taskService = new TaskService())
                    {
                        if (startOnStartup == false)
                        {
                            taskService.RootFolder.DeleteTask(TaskName, false);
                            return;
                        }

                        TaskDefinition task = taskService.NewTask();

                        LogonTrigger trigger = new LogonTrigger()
                        {
                            Delay = TimeSpan.FromSeconds(10)
                        };

                        task.Triggers.Add(trigger);

                        task.Principal.RunLevel = TaskRunLevel.Highest;
                        task.Settings.DisallowStartIfOnBatteries = false;
                        task.Settings.StopIfGoingOnBatteries = false;
                        task.Settings.RunOnlyIfNetworkAvailable = false;
                        task.Settings.RunOnlyIfIdle = false;
                        task.Settings.ExecutionTimeLimit = TimeSpan.Zero;

                        task.Actions.Add(
                            new ExecAction(
                               Assembly.GetExecutingAssembly().Location,
                                "--taskscheduler",
                                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));

                        task.RegistrationInfo.Description = $"{TaskName} startup.";

                        taskService.RootFolder.RegisterTaskDefinition(TaskName, task);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to create Windows startup task.");
                }
            });
        }

    }
}
