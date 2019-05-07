using Caliburn.Micro;
using WLVPN.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Action = System.Action;

namespace WLVPN.ViewModels
{
    public class BusyViewModel : PropertyChangedBase, IBusyManager, IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;
        private System.Action _callback;

        private bool _isActive = false;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                NotifyOfPropertyChange(nameof(IsActive));
                IsActiveChanged?.Invoke(this, value);
            }
        }

        public string BusyText { get; set; }

        public bool IsCancellable { get; set; }

        public TimeSpan DelayRemained { get; set; }

        public void Activate(string busyText, Action callback = null)
        {
            BusyText = busyText;
            IsCancellable = callback != null;
            IsActive = true;
            _callback = callback;
            DelayRemained = TimeSpan.FromMilliseconds(0);
        }

        public void Dismiss()
        {
            IsActive = false;
            DelayRemained = TimeSpan.FromMilliseconds(0);
        }

        public event EventHandler<bool> IsActiveChanged;

        public void Cancel()
        {
            try
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource?.Cancel();
                }
                _callback?.Invoke();
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
        }

        public async Task ActivateWithDelay(string busyText, TimeSpan delayPeriod, TimeSpan interval, Action intervalAction, Action callback = null)
        {
            BusyText = busyText;
            IsCancellable = callback != null;
            _callback = callback;
            IsActive = true;
            DelayRemained = delayPeriod;
            _cancellationTokenSource = new CancellationTokenSource();

            while (DelayRemained.TotalSeconds > 0 && IsActive)
            {
                try
                {
                    await Task.Delay(interval, _cancellationTokenSource.Token);
                }
                catch
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                    return;
                }
                DelayRemained = DelayRemained.Subtract(interval);
                intervalAction.Invoke();
            }
            try
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
        }

        public void ChangeBusyText(string text)
        {
            BusyText = text;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                _cancellationTokenSource?.Dispose();
            }
            catch
            {
            }
        }
    }
}