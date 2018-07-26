// <copyright file="SparkleUIFactory.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NetSparkle;
using NetSparkle.Interfaces;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.Utilities
{
    public class SparkleUIFactory : DefaultUIFactory, IUIFactory
    {
        public override IUpdateAvailable CreateSparkleForm(Sparkle sparkle, AppCastItem[] updates, Icon applicationIcon, bool isUpdateAlreadyDownloaded = false)
        {
            return new NetSparkleUpdateDialog(updates.OrderByDescending(p => p.Version).FirstOrDefault());
        }
    }
}
