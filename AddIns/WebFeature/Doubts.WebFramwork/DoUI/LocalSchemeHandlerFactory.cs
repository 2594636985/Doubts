using Chromium;
using Chromium.WebBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.WebFramework.DoUI
{
    internal class LocalSchemeHandlerFactory : CfxSchemeHandlerFactory
    {
        internal LocalSchemeHandlerFactory()
        {
            this.Create += LocalSchemeHandlerFactory_Create;
        }

        private void LocalSchemeHandlerFactory_Create(object sender, Chromium.Event.CfxSchemeHandlerFactoryCreateEventArgs e)
        {
            if (e.SchemeName.Equals("local") && e.Browser != null)
            {
                var browser = ChromiumWebBrowser.GetBrowser(e.Browser.Identifier);
                var handler = new LocalResourceHandler(browser);
                e.SetReturnValue(handler);
            }
        }
    }
}
