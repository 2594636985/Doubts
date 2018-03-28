using Hydra.AomiCss.Printing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hydra.AomiCss
{
    public sealed class AoGenius
    {
        public static void Printing(string html, string name)
        {
            PrintingEngine printingEngine = new DriverPrintingEngine(name);

            printingEngine.Engine(html);
        }
    }
}
