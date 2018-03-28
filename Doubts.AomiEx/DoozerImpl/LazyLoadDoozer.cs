using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.AomiEx.DoozerImpl
{
    sealed class LazyLoadDoozer : IDoozer
    {
        AddIn addIn;
        string name;
        string className;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public LazyLoadDoozer(AddIn addIn, AddInProperties properties)
        {
            this.addIn = addIn;
            this.name = properties["name"];
            this.className = properties["class"];
        }

        /// <summary>
        /// Gets if the doozer handles codon conditions on its own.
        /// If this property return false, the item is excluded when the condition is not met.
        /// </summary>
        public bool HandleConditions
        {
            get
            {
                IDoozer doozer = (IDoozer)addIn.CreateObject(className);

                if (doozer == null)
                {
                    return false;
                }

                addIn.AddInTree.Doozers[name] = doozer;

                return doozer.HandleConditions;
            }
        }

        public object BuildItem(BuildItemArgs args)
        {
            IDoozer doozer = (IDoozer)addIn.CreateObject(className);
            if (doozer == null)
            {
                return null;
            }
            addIn.AddInTree.Doozers[name] = doozer;
            return doozer.BuildItem(args);
        }

        public override string ToString()
        {
            return String.Format("[LazyLoadDoozer: className = {0}, name = {1}]", className, name);
        }
    }
}
