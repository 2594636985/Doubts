using Chromium;
using Chromium.WebBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Chromium.Event;
using Chromium.WebBrowser.Event;
using System.IO;
using System.Reflection;

namespace Doubts.ChromiumSubProcess
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            if (CfxRuntime.PlatformArch == CfxPlatformArch.x64)
                CfxRuntime.LibCefDirPath = @"cef64";
            else
                CfxRuntime.LibCefDirPath = @"cef";

            ChromiumWebBrowser.OnBeforeCfxInitialize += ChromiumWebBrowser_OnBeforeCfxInitialize;
            ChromiumWebBrowser.Initialize();

            CfxRuntime.Shutdown();
        }

        private static void ChromiumWebBrowser_OnBeforeCfxInitialize(OnBeforeCfxInitializeEventArgs e)
        {
            if (CfxRuntime.PlatformArch == CfxPlatformArch.x64)
            {
                e.Settings.LocalesDirPath = Path.GetFullPath(@"cef64\locales");
                e.Settings.ResourcesDirPath = Path.GetFullPath(@"cef64");
            }
            else
            {
                e.Settings.LocalesDirPath = Path.GetFullPath(@"cef\locales");
                e.Settings.ResourcesDirPath = Path.GetFullPath(@"cef");
            }
        }
    }
}
