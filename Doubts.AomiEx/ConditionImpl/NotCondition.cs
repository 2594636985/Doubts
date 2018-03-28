using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Doubts.AomiEx.ConditionImpl
{
    public class NotCondition : ICondition
    {
        ICondition condition;

        public string Name
        {
            get
            {
                return "Not " + condition.Name;
            }
        }

        ConditionFailedAction action = ConditionFailedAction.Exclude;
        public ConditionFailedAction Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }

        public NotCondition(ICondition condition)
        {
            this.condition = condition;
        }

        public bool IsValid(object parameter)
        {
            return !condition.IsValid(parameter);
        }

        public static ICondition Read(XmlReader reader, AddIn addIn)
        {
            return new NotCondition(Condition.ReadConditionList(reader, "Not", addIn)[0]);
        }
    }
}
