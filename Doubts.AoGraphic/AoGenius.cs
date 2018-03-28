using Doubts.AoGraphic.Printing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.AoGraphic
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
