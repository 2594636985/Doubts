using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Doubts.AomiEx
{
    public class Codon
    {
        AddIn addIn;
        string name;
        AddInProperties properties;
        ReadOnlyCollection<ICondition> conditions;

        public string Name
        {
            get { return name; }
        }

        public AddIn AddIn
        {
            get { return addIn; }
        }

        public string Id
        {
            get { return properties["id"]; }
        }

        public string InsertAfter
        {
            get { return properties["insertafter"]; }
        }

        public string InsertBefore
        {
            get { return properties["insertbefore"]; }
        }

        public string this[string key]
        {
            get { return properties[key]; }
        }

        public AddInProperties Properties
        {
            get { return properties; }
        }

        public ReadOnlyCollection<ICondition> Conditions
        {
            get { return conditions; }
        }

        public Codon(AddIn addIn, string name, AddInProperties properties, ReadOnlyCollection<ICondition> conditions)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (properties == null)
                throw new ArgumentNullException("properties");
            this.addIn = addIn;
            this.name = name;
            this.properties = properties;
            this.conditions = conditions;
        }

        internal object BuildItem(BuildItemArgs args)
        {
            IDoozer doozer;
            if (!addIn.AddInTree.Doozers.TryGetValue(Name, out doozer))
                throw new AddInException("Doozer " + Name + " not found! " + ToString());

            if (!doozer.HandleConditions)
            {
                ConditionFailedAction action = Condition.GetFailedAction(args.Conditions, args.Parameter);
                if (action != ConditionFailedAction.Nothing)
                {
                    return null;
                }
            }


            return doozer.BuildItem(args);
        }

        public override string ToString()
        {
            return String.Format("[Codon: name = {0}, id = {1}, addIn={2}]",  name,  Id,  addIn.FileName);
        }
    }
}
