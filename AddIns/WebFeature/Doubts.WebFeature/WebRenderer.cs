using System;
using System.Drawing;
using System.Windows.Forms;
using Chromium;
using Chromium.Event;
using Chromium.WebBrowser;
using System.Diagnostics;
using Chromium.WebBrowser.Event;
using Doubts;
using System.IO;
using System.Reflection;

namespace Doubts.WebFeature
{
    public class WebRenderer : Doubts.Framework.Renderer
    {
        public override void Main(string[] args)
        {
            string assemblyDir = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (CfxRuntime.PlatformArch == CfxPlatformArch.x64)
                CfxRuntime.LibCefDirPath = Path.Combine(assemblyDir, @"cef64");
            else
                CfxRuntime.LibCefDirPath = Path.Combine(assemblyDir, @"cef");

            ChromiumWebBrowser.OnBeforeCfxInitialize += ChromiumWebBrowser_OnBeforeCfxInitialize;
            ChromiumWebBrowser.OnBeforeCommandLineProcessing += ChromiumWebBrowser_OnBeforeCommandLineProcessing;
            ChromiumWebBrowser.Initialize();


            FrmMain frmMain = new FrmMain();

            frmMain.Show();

            Application.Run(frmMain);

            CfxRuntime.Shutdown();

        }

        private void ChromiumWebBrowser_OnBeforeCommandLineProcessing(CfxOnBeforeCommandLineProcessingEventArgs e)
        {
            Console.WriteLine("ChromiumWebBrowser_OnBeforeCommandLineProcessing");
            Console.WriteLine(e.CommandLine.CommandLineString);
        }

        private void ChromiumWebBrowser_OnBeforeCfxInitialize(OnBeforeCfxInitializeEventArgs e)
        {
            string assemblyDir = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            e.Settings.BrowserSubprocessPath = Path.Combine(assemblyDir, "Doubts.ChromiumSubProcess.exe");

            if (CfxRuntime.PlatformArch == CfxPlatformArch.x64)
            {
                e.Settings.LocalesDirPath = Path.Combine(assemblyDir, @"cef64\locales");
                e.Settings.ResourcesDirPath = Path.Combine(assemblyDir, @"cef64");
            }
            else
            {
                e.Settings.LocalesDirPath = Path.Combine(assemblyDir, @"cef\locales");
                e.Settings.ResourcesDirPath = Path.Combine(assemblyDir, @"cef");
            }
        }
    }
}
