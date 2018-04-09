using Chromium;
using Chromium.WebBrowser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Doubts.WebFramework.DoUI
{
    internal class LocalResourceHandler : CfxResourceHandler
    {
        private int readResponseStreamOffset;
        private string requestFile = null;
        private string requestUrl = null;
        private WebResource webResource;
        private ChromiumWebBrowser browser;
        private GCHandle gcHandle;

        internal LocalResourceHandler(ChromiumWebBrowser browser)
        {
            this.gcHandle = GCHandle.Alloc(this);
            this.browser = browser;
            this.GetResponseHeaders += LocalResourceHandler_GetResponseHeaders;
            this.ProcessRequest += LocalResourceHandler_ProcessRequest;
            this.ReadResponse += LocalResourceHandler_ReadResponse;
            this.CanGetCookie += (s, e) => e.SetReturnValue(false);
            this.CanSetCookie += (s, e) => e.SetReturnValue(false);

        }

        private void LocalResourceHandler_ProcessRequest(object sender, Chromium.Event.CfxProcessRequestEventArgs e)
        {

            this.readResponseStreamOffset = 0;

            var request = e.Request;
            var callback = e.Callback;

            var uri = new Uri(request.Url);

            this.requestUrl = request.Url;

            var localPath = uri.LocalPath;
            if (localPath.StartsWith("/"))
                localPath = $".{localPath}";

            var fileName = System.IO.Path.GetFullPath(localPath);

            this.requestFile = request.Url;

            if (File.Exists(fileName))
            {
                using (var stream = File.OpenRead(fileName))
                using (var reader = new BinaryReader(stream))
                {
                    var buff = reader.ReadBytes((int)reader.BaseStream.Length);
                    webResource = new WebResource(buff, MimeHelper.GetMimeType(Path.GetExtension(fileName)));

                    reader.Close();
                    stream.Close();
                }

                Console.WriteLine($"[加载]:\t{requestUrl}\t->\t{fileName}");
            }
            else
            {
                Console.WriteLine($"[未找到]:\t{requestUrl}");
            }

            callback.Continue();
            e.SetReturnValue(true);

        }

        private void LocalResourceHandler_GetResponseHeaders(object sender, Chromium.Event.CfxGetResponseHeadersEventArgs e)
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


        private void LocalResourceHandler_ReadResponse(object sender, Chromium.Event.CfxReadResponseEventArgs e)
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
