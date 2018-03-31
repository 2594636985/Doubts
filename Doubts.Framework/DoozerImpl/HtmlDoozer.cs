using Doubts.AomiEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Doubts.Framework.DoozerImpl
{
    public class HtmlDoozer : IDoozer
    {
        public bool HandleConditions
        {
            get
            {
                return false;
            }
        }

        public object BuildItem(BuildItemArgs args)
        {
            string html = string.Empty;

            Codon codon = args.Codon;

            string location = codon.Properties["location"];

            if (!string.IsNullOrWhiteSpace(location))
            {
                Stream stream = codon.AddIn.FindResources(location);
                if (stream != null)
                {
                    using (stream)
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        html = sr.ReadToEnd();
                    }
                }
            }
            return html;
        }
    }
}
