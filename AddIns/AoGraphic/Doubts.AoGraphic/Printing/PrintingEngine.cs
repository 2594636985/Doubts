using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.AoGraphic.Printing
{
    public abstract class PrintingEngine 
    {
        public string Name { set; get; }

        public int LeftMargin { set; get; }

        public int TopMargin { set; get; }

        public int Copies { set; get; }

        protected int LimitPaperWidth { set; get; }

        public int PaperWidth { set; get; }

        public PrintingEngine() : this(null)
        { }

        public PrintingEngine(string name)
        {
            this.Name = name;
            this.LeftMargin = 0;
            this.TopMargin = 0;
            this.Copies = 1;
        }

        public abstract void Engine(string html);

    }
}
