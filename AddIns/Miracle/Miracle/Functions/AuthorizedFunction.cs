using Doubts.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Miracle.Functions
{
    public class AuthorizedFunction : Function<string>
    {
        public override string Execute(params object[] args)
        {
            string html = string.Empty;

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Miracle.Views.Authorized.html");

            if (stream != null)
            {
                using (stream)
                using (StreamReader sr = new StreamReader(stream))
                {
                    html = sr.ReadToEnd();
                }
            }

            return html;
        }
    }
}
