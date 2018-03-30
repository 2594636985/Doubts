using Doubts.AomiEx;
using Doubts.Framework;
using Doubts.WebFeature.Properties;
using Doubts.WebFramework.DoUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Doubts.WebFeature
{
    public partial class FrmMain : ChromiumForm
    {
        private NotifyIcon notifyIcon;
        public FrmMain()
        {
            InitializeComponent();
            InitializeFrmMain();
            InitializeNotifyIcon();

        }

        private void InitializeFrmMain()
        {
            this.SplashBackColor = Color.LightGray;
            this.SplashImageLayout = ImageLayout.Center;
            this.SplashImage = Image.FromStream(this.GetType().Assembly.GetManifestResourceStream("Doubts.WebFeature.Resources.loading.gif"));
        }

        /// <summary>
        /// 初始化托盘
        /// </summary>
        private void InitializeNotifyIcon()
        {
            this.notifyIcon = new NotifyIcon();

            MenuItem openMenuItem = new MenuItem(StringResources.NotifyIcon_Open);

            openMenuItem.Click += OpenMenuItem_Click;

            MenuItem exitMenuItem = new MenuItem(StringResources.NotifyIcon_Exit);

            exitMenuItem.Click += ExitMenuItem_Click;

            MenuItem[] mItems = new MenuItem[] { openMenuItem, exitMenuItem };

            this.notifyIcon.ContextMenu = new ContextMenu(mItems);
            this.notifyIcon.Icon = new Icon(this.GetType().Assembly.GetManifestResourceStream("Doubts.WebFeature.Resources.doubts.ico"));
            this.notifyIcon.Visible = true;

            this.notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {

        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            string html = string.Empty;

            Function<string> function = AddInManager.GetSingleInstance<Function<string>>("/Doubts/WebProgram", null, false);

            if (function != null)
            {
                html = function.Execute();
            }
            else
            {
                using (StreamReader sr = new StreamReader(this.GetType().Assembly.GetManifestResourceStream("Doubts.WebFeature.404.html")))
                {
                    html = sr.ReadToEnd();
                }
            }

            this.LoadString(html);
        }
    }
}
