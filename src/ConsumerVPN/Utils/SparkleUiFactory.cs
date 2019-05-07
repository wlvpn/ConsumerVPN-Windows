using NetSparkle;
using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WLVPN.Views;

namespace WLVPN.Utils
{
    public class SparkleUIFactory : DefaultUIFactory, IUIFactory
    {
        public override IUpdateAvailable CreateSparkleForm(Sparkle sparkle, AppCastItem[] updates, Icon applicationIcon, bool isUpdateAlreadyDownloaded = false)
        {
            return new NetSparkleUpdateDialog(updates.FirstOrDefault());
        }
    }
}
