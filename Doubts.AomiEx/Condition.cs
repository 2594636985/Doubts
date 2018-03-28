using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Doubts.AomiEx.ConditionImpl;
using Doubts.AomiEx.Properties;

namespace Doubts.AomiEx
{
    public class Condition : ICondition
    {
        string name;
        AddInProperties properties;
        ConditionFailedAction action;

        public AddIn AddIn { get; private set; }

        public ConditionFailedAction Action
        {
            get { return action; }
            set
            {
                action = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string this[string key]
        {
            get
            {
                return properties[key];
            }
        }

        public AddInProperties Properties
        {
            get
            {
                return properties;
            }
        }

        public Condition(string name, AddInProperties properties, AddIn addIn)
        {
            this.AddIn = addIn;
            this.name = name;
            this.properties = properties;
            action = properties.Get("action", ConditionFailedAction.Exclude);
        }

        public bool IsValid(object parameter)
        {
            try
            {
                return this.AddIn.AddInTree.ConditionEvaluators[name].IsValid(parameter, this);
            }
            catch (KeyNotFoundException)
            {
                throw new AddInException(StringResources.AddIn_Condition_Evaluator_NotFound);
            }
        }

        public static ICondition Read(XmlReader reader, AddIn addIn)
        {
            AddInProperties properties = AddInProperties.ReadFromAttributes(reader);
            string conditionName = properties["name"];
            return new Condition(conditionName, properties, addIn);
        }

        public static ICondition ReadComplexCondition(XmlReader reader, AddIn addIn)
        {
            AddInProperties properties = AddInProperties.ReadFromAttributes(reader);
            reader.Read();
            ICondition condition = null;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "And":
                                condition = AndCondition.Read(reader, addIn);
                                goto exit;
                            case "Or":
                                condition = OrCondition.Read(reader, addIn);
                                goto exit;
                            case "Not":
                                condition = NotCondition.Read(reader, addIn);
                                goto exit;
                            default:
                                throw new AddInException(string.Format(StringResources.AddIn_Xml_ComplexCondition_InValidate, reader.LocalName));
                        }
                }
            }
            exit:
            if (condition != null)
            {
                ConditionFailedAction action = properties.Get("action", ConditionFailedAction.Exclude);
                condition.Action = action;
            }
            return condition;
        }

        public static ICondition[] ReadConditionList(XmlReader reader, string endElement, AddIn addIn)
        {
            List<ICondition> conditions = new List<ICondition>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == endElement)
                        {
                            return conditions.ToArray();
                        }
                        break;
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "And":
                                conditions.Add(AndCondition.Read(reader, addIn));
                                break;
                            case "Or":
                                conditions.Add(OrCondition.Read(reader, addIn));
                                break;
                            case "Not":
                                conditions.Add(NotCondition.Read(reader, addIn));
                                break;
                            case "Condition":
                                conditions.Add(Condition.Read(reader, addIn));
                                break;
                            default:
                                throw new AddInException(string.Format(StringResources.AddIn_Xml_Condition_InValidate, reader.LocalName));
                        }
                        break;
                }
            }
            return conditions.ToArray();
        }

        public static ConditionFailedAction GetFailedAction(IEnumerable<ICondition> conditionList, object parameter)
        {
            ConditionFailedAction action = ConditionFailedAction.Nothing;
            foreach (ICondition condition in conditionList)
            {
                if (!condition.IsValid(parameter))
                {
                    if (condition.Action == ConditionFailedAction.Disable)
                    {
                        action = ConditionFailedAction.Disable;
                    }
                    else
                    {
                        return ConditionFailedAction.Exclude;
                    }
                }
            }
            return action;
        }
    }
}
