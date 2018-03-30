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
using Doubts.WebFramework;

namespace Doubts.WebFeature
{
    public class WebRenderer : Doubts.Framework.Renderer
    {
        public override void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string assemblyDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            if (DoubtsUILauncher.InitializeChromium(assemblyDir, BeforeChromiumInitialize))
            {
                //初始化成功，加载程序集内嵌的资源到运行时中
                DoubtsUILauncher.RegisterEmbeddedScheme(Assembly.GetExecutingAssembly(), domainName: "res.welcome.local");

                //启动主窗体
                Application.Run(new FrmMain());
            }
        }

        private void BeforeChromiumInitialize(OnBeforeCfxInitializeEventArgs e)
        {
            e.Settings.LogSeverity = Chromium.CfxLogSeverity.Default;
        }
    }
}
