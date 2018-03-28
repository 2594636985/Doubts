using Chromium;
using Chromium.WebBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Chromium.Event;
using Chromium.WebBrowser.Event;

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
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            if (CfxRuntime.PlatformArch == CfxPlatformArch.x64)
                CfxRuntime.LibCefDirPath = @"cef64";
            else
                CfxRuntime.LibCefDirPath = @"cef";

            Chromium.WebBrowser.ChromiumWebBrowser.OnBeforeCfxInitialize += ChromiumWebBrowser_OnBeforeCfxInitialize;
            ChromiumWebBrowser.OnBeforeCommandLineProcessing += ChromiumWebBrowser_OnBeforeCommandLineProcessing;
            Chromium.WebBrowser.ChromiumWebBrowser.Initialize();
       
            CfxRuntime.Shutdown();
        }

        private static void ChromiumWebBrowser_OnBeforeCfxInitialize(OnBeforeCfxInitializeEventArgs e)
        {
            if (CfxRuntime.PlatformArch == CfxPlatformArch.x64)
            {
                e.Settings.LocalesDirPath = System.IO.Path.GetFullPath(@"cef64\locales");
                e.Settings.ResourcesDirPath = System.IO.Path.GetFullPath(@"cef64");
            }
            else
            {
                e.Settings.LocalesDirPath = System.IO.Path.GetFullPath(@"cef\locales");
                e.Settings.ResourcesDirPath = System.IO.Path.GetFullPath(@"cef");
            }
        }

        private static void ChromiumWebBrowser_OnBeforeCommandLineProcessing(CfxOnBeforeCommandLineProcessingEventArgs e)
        {
            
        }
    }
}
