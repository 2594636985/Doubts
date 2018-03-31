using Chromium;
using Chromium.WebBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Doubts.WebFramework.DoUI
{
    internal class EmbeddedSchemeHandlerFactory : CfxSchemeHandlerFactory
    {
        public string SchemeName { private set; get; }

        public string DomainName { private set; get; }

        private readonly Assembly resourceAssembly;

        internal EmbeddedSchemeHandlerFactory(string schemeName, string domainName, Assembly resourceAssembly)
        {
            this.resourceAssembly = resourceAssembly;
            this.SchemeName = schemeName;
            this.DomainName = domainName;
            this.Create += EmbeddedSchemeHandlerFactory_Create;
        }

        private void EmbeddedSchemeHandlerFactory_Create(object sender, Chromium.Event.CfxSchemeHandlerFactoryCreateEventArgs e)
        {
            if (e.SchemeName == SchemeName && e.Browser != null)
            {
                ChromiumWebBrowser browser = ChromiumWebBrowser.GetBrowser(e.Browser.Identifier);
                EmbeddedResourceHandler handler = new EmbeddedResourceHandler(resourceAssembly, browser, DomainName);

                e.SetReturnValue(handler);
            }

        }
    }
}
