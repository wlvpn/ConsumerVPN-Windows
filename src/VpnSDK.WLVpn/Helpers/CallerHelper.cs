// <copyright file="CallerHelper.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VpnSDK.WLVpn.Helpers
{
    /// <summary>
    /// Class CallerHelper. Debug utility to find what exactly called a method.
    /// </summary>
    public static class CallerHelper
    {
        public static string WhoInvoked()
        {
            StackFrame frame = new StackFrame(2, true);
            return
                $"Method: {frame.GetMethod()} Line: {frame.GetFileLineNumber()} - {frame.GetFileName()} Class: {frame.GetMethod().DeclaringType}";
        }
    }
}