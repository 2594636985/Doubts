using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Doubts.AomiEx
{
    public class AddInPath
    {
        string name;
        AddIn addIn;
        List<List<Codon>> codons = new List<List<Codon>>();

        public AddIn AddIn
        {
            get
            {
                return addIn;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public IEnumerable<Codon> Codons
        {
            get
            {
                return
                    from list in codons
                    from c in list
                    select c;
            }
        }

        public IEnumerable<IEnumerable<Codon>> GroupedCodons
        {
            get
            {
                return codons.AsReadOnly();
            }
        }

        public AddInPath(string name, AddIn addIn)
        {
            this.addIn = addIn;
            this.name = name;
        }

        public static void SetUp(AddInPath addInPath, XmlReader reader, string endElement)
        {
            addInPath.DoSetUp(reader, endElement, addInPath.addIn);
        }

        void DoSetUp(XmlReader reader, string endElement, AddIn addIn)
        {
            Stack<ICondition> conditionStack = new Stack<ICondition>();
            List<Codon> innerCodons = new List<Codon>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == "Condition" || reader.LocalName == "ComplexCondition")
                        {
                            conditionStack.Pop();
                        }
                        else if (reader.LocalName == endElement)
                        {
                            if (innerCodons.Count > 0)
                                this.codons.Add(innerCodons);
                            return;
                        }
                        break;
                    case XmlNodeType.Element:
                        string elementName = reader.LocalName;
                        if (elementName == "Condition")
                        {
                            conditionStack.Push(Condition.Read(reader, addIn));
                        }
                        else if (elementName == "ComplexCondition")
                        {
                            conditionStack.Push(Condition.ReadComplexCondition(reader, addIn));
                        }
                        else
                        {
                            Codon newCodon = new Codon(this.AddIn, elementName, AddInProperties.ReadFromAttributes(reader), conditionStack.ToList().AsReadOnly());
                            innerCodons.Add(newCodon);
                            if (!reader.IsEmptyElement)
                            {
                                AddInPath subPath = this.AddIn.GetExtensionPath(this.Name + "/" + newCodon.Id);
                                subPath.DoSetUp(reader, elementName, addIn);
                            }
                        }
                        break;
                }
            }
            if (innerCodons.Count > 0)
                this.codons.Add(innerCodons);
        }
    }
}
