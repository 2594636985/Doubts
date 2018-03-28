using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.AomiEx.DoozerImpl
{
    public class ClassDoozer : IDoozer
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
            Codon codon = args.Codon;

            return codon.AddIn.CreateObject(codon.Properties["class"]);
        }
    }
}
