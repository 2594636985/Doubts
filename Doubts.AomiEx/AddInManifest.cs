using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using Doubts.AomiEx.Properties;

namespace Doubts.AomiEx
{
    /// <summary>
    /// 插件清单类
    /// </summary>
    public class AddInManifest
    {
        private List<AddInReference> dependencies = new List<AddInReference>();
        private List<AddInReference> conflicts = new List<AddInReference>();
        private Dictionary<string, Version> identities = new Dictionary<string, Version>();
        private Version primaryVersion;
        private string primaryIdentity;

        public string PrimaryIdentity
        {
            get { return primaryIdentity; }
        }

        public Version PrimaryVersion
        {
            get { return primaryVersion; }
        }

        public Dictionary<string, Version> Identities
        {
            get { return identities; }
        }
        /// <summary>
        /// 发生冲突的插件
        /// </summary>
        public ReadOnlyCollection<AddInReference> Conflicts
        {
            get { return conflicts.AsReadOnly(); }
        }

        /// <summary>
        /// 发生依懒的插件
        /// </summary>
        public ReadOnlyCollection<AddInReference> Dependencies
        {
            get { return dependencies.AsReadOnly(); }
        }

        public void ReadManifestSection(XmlReader reader, string hintPath)
        {
            if (reader.AttributeCount != 0)
            {
                throw new AddInException(StringResources.AddIn_Manifest_HaveAttribute);
            }
            if (reader.IsEmptyElement)
            {
                throw new AddInException(StringResources.AddIn_Manifest_EmptyElement);
            }
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == "Manifest")
                        {
                            return;
                        }
                        break;
                    case XmlNodeType.Element:
                        string nodeName = reader.LocalName;
                        AddInProperties properties = AddInProperties.ReadFromAttributes(reader);
                        switch (nodeName)
                        {
                            case "Identity":
                                AddIdentity(properties["name"], properties["version"], hintPath);
                                break;
                            case "Dependency":
                                dependencies.Add(AddInReference.Create(properties, hintPath));
                                break;
                            case "Conflict":
                                conflicts.Add(AddInReference.Create(properties, hintPath));
                                break;
                            default:
                                throw new AddInException("Unknown node in Manifest section:" + nodeName);
                        }
                        break;
                }
            }
        }

        private void AddIdentity(string name, string version, string hintPath)
        {
            if (name.Length == 0)
                throw new AddInException("Identity needs a name");
            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '.' && c != '_')
                {
                    throw new AddInException("Identity name contains invalid character: '" + c + "'");
                }
            }
            Version v = AddInReference.ParseVersion(version, hintPath);

            if (primaryVersion == null)
            {
                primaryVersion = v;
            }

            if (primaryIdentity == null)
            {
                primaryIdentity = name;
            }

            identities.Add(name, v);
        }

    }
}
