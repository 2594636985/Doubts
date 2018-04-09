using Chromium;
using Chromium.WebBrowser;
using Doubts.AomiEx;
using Doubts.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Doubts.WebFramework.DoUI
{
    internal class DoubtsResourceHandler : CfxResourceHandler
    {
        private int readResponseStreamOffset;
        private string requestFile = null;
        private string requestUrl = null;
        private WebResource webResource;
        private ChromiumWebBrowser browser;
        private GCHandle gcHandle;

        internal DoubtsResourceHandler(ChromiumWebBrowser browser)
        {
            this.gcHandle = GCHandle.Alloc(this);

            this.browser = browser;

            this.GetResponseHeaders += DoubtsResourceHandler_GetResponseHeaders;
            this.ProcessRequest += DoubtsResourceHandler_ProcessRequest;
            this.ReadResponse += DoubtsResourceHandler_ReadResponse;
            this.CanGetCookie += (s, e) => e.SetReturnValue(false);
            this.CanSetCookie += (s, e) => e.SetReturnValue(false);

        }

        private void DoubtsResourceHandler_ProcessRequest(object sender, Chromium.Event.CfxProcessRequestEventArgs e)
        {
            this.readResponseStreamOffset = 0;

            CfxRequest request = e.Request;
            CfxCallback callback = e.Callback;

            Uri uri = new Uri(request.Url);

            this.requestUrl = request.Url;

            Function<string> function = AddInManager.GetEntityInstance<Function<string>>(string.Format("/{0}{1}", uri.Host, uri.LocalPath));

            if (function != null)
            {
                string html = function.Execute();

                this.webResource = new WebResource(html);

                Console.WriteLine($"[加载]:\t{requestUrl}\t->\t{uri.AbsolutePath}");
            }
            else
            {
                Console.WriteLine($"[未找到]:\t{requestUrl}");
            }

            callback.Continue();
            e.SetReturnValue(true);

        }

        private void DoubtsResourceHandler_GetResponseHeaders(object sender, Chromium.Event.CfxGetResponseHeadersEventArgs e)
        {
            if (webResource == null)
            {
                e.Response.Status = 404;
            }
            else
            {
                e.ResponseLength = webResource.data.Length;
                e.Response.MimeType = webResource.mimeType;
                e.Response.Status = 200;

                if (!browser.webResources.ContainsKey(requestUrl))
                {
                    browser.SetWebResource(requestUrl, webResource);
                }

            }
        }


        private void DoubtsResourceHandler_ReadResponse(object sender, Chromium.Event.CfxReadResponseEventArgs e)
        {
            int bytesToCopy = webResource.data.Length - readResponseStreamOffset;

            if (bytesToCopy > e.BytesToRead)
                bytesToCopy = e.BytesToRead;

            Marshal.Copy(webResource.data, readResponseStreamOffset, e.DataOut, bytesToCopy);

            e.BytesRead = bytesToCopy;

            readResponseStreamOffset += bytesToCopy;

            e.SetReturnValue(true);

            if (readResponseStreamOffset == webResource.data.Length)
            {
                gcHandle.Free();

                Console.WriteLine($"[完成]:\t{requestUrl}");
            }
        }
    }
}
