using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Doubts.AomiEx.ConditionImpl
{
    public class AndCondition : ICondition
    {
        ICondition[] conditions;

        public string Name
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < conditions.Length; ++i)
                {
                    sb.Append(conditions[i].Name);
                    if (i + 1 < conditions.Length)
                    {
                        sb.Append(" And ");
                    }
                }
                return sb.ToString();
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

        public AndCondition(ICondition[] conditions)
        {
            this.conditions = conditions;
        }

        public bool IsValid(object parameter)
        {
            foreach (ICondition condition in conditions)
            {
                if (!condition.IsValid(parameter))
                {
                    return false;
                }
            }
            return true;
        }

        public static ICondition Read(XmlReader reader, AddIn addIn)
        {
            return new AndCondition(Condition.ReadConditionList(reader, "And", addIn));
        }
    }

}
