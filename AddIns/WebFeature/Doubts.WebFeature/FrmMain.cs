using Doubts.AomiEx;
using Doubts.WebFramework.DoUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Doubts.WebFeature
{
    public partial class FrmMain : ChromiumForm
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddInManager.GetSingleInstance<>
            this.LoadUrl("www.baidu.com");
        }
    }
}
