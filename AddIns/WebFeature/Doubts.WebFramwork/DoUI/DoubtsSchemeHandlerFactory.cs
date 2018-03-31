using Chromium;
using Chromium.WebBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.WebFramework.DoUI
{
    public class DoubtsSchemeHandlerFactory : CfxSchemeHandlerFactory
    {
        internal DoubtsSchemeHandlerFactory()
        {
            this.Create += LocalSchemeHandlerFactory_Create;
        }

        private void LocalSchemeHandlerFactory_Create(object sender, Chromium.Event.CfxSchemeHandlerFactoryCreateEventArgs e)
        {
            if (e.SchemeName.Equals("doubts") && e.Browser != null)
            {
                var browser = ChromiumWebBrowser.GetBrowser(e.Browser.Identifier);
                var handler = new DoubtsResourceHandler(browser);
                e.SetReturnValue(handler);
            }
        }
    }
}
