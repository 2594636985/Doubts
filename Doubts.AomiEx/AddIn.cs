using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Doubts.AomiEx
{
    public sealed class AddIn
    {
        private volatile bool dependenciesLoaded;

        private IAddInTree addInTree;
        private string customErrorMessage;
        private bool enabled;
        private AddInProperties properties = new AddInProperties();
        private AddInAction action = AddInAction.Disable;
        private AddInManifest manifest = new AddInManifest();
        private List<Runtime> runtimes = new List<Runtime>();
        private List<string> bitmapResources = new List<string>();
        private List<string> stringResources = new List<string>();
        private Dictionary<string, AddInPath> paths = new Dictionary<string, AddInPath>();

        internal string addInFileName;


        public AddInAction Action
        {
            get { return action; }
            set { action = value; }
        }

        public string Name
        {
            get { return properties["name"]; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                this.Action = value ? AddInAction.Enable : AddInAction.Disable;
            }
        }

        public string CustomErrorMessage
        {
            get { return customErrorMessage; }
            internal set
            {
                if (value != null)
                {
                    Enabled = false;
                    Action = AddInAction.CustomError;
                }
                customErrorMessage = value;
            }
        }

        public string FileName
        {
            get { return addInFileName; }
            set { addInFileName = value; }
        }

        public AddInManifest Manifest
        {
            get { return manifest; }
        }

        public AddInProperties Properties
        {
            set { this.properties = value; }
            get { return this.properties; }
        }


        public IAddInTree AddInTree
        {
            get { return addInTree; }
        }

        public List<string> BitmapResources
        {
            get { return bitmapResources; }
            set { bitmapResources = value; }
        }

        public List<string> StringResources
        {
            get { return stringResources; }
            set { stringResources = value; }
        }

        public ReadOnlyCollection<Runtime> Runtimes
        {
            get { return runtimes.AsReadOnly(); }
        }

        public Dictionary<string, AddInPath> Paths
        {
            get { return paths; }
        }

        internal AddIn(IAddInTree addInTree)
        {
            if (addInTree == null)
                throw new ArgumentNullException("addInTree");

            this.addInTree = addInTree;
        }

        public AddInPath GetExtensionPath(string pathName)
        {
            if (!paths.ContainsKey(pathName))
            {
                return paths[pathName] = new AddInPath(pathName, this);
            }
            return paths[pathName];
        }

        public object CreateObject(string className)
        {
            Type t = FindType(className);
            if (t != null)
                return Activator.CreateInstance(t);
            else
                return null;
        }

        public Type FindType(string className)
        {
            foreach (Runtime runtime in runtimes)
            {
                if (!runtime.IsHostApplicationAssembly)
                {
                    LoadDependencies();
                }

                Type t = runtime.FindType(className);

                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }

        public void LoadRuntimeAssemblies()
        {
            LoadDependencies();

            foreach (Runtime runtime in runtimes)
            {
                if (runtime.IsActive)
                    runtime.Load();
            }
        }

        private void LoadDependencies()
        {
            if (!dependenciesLoaded)
            {

                AssemblyLocator.Init();

                foreach (AddInReference r in manifest.Dependencies)
                {
                    if (r.RequirePreload)
                    {
                        bool found = false;
                        foreach (AddIn addIn in AddInTree.AddIns)
                        {
                            if (addIn.Manifest.Identities.ContainsKey(r.Name))
                            {
                                found = true;

                                addIn.LoadRuntimeAssemblies();
                            }
                        }
                        if (!found)
                        {
                            throw new AddInException("Cannot load run-time dependency for " + r.ToString());
                        }
                    }
                }
                dependenciesLoaded = true;
            }
        }

        /// <summary>
        /// 通一个文件来加载插件信息
        /// </summary>
        /// <param name="addInTree"></param>
        /// <param name="fileName"></param>
        /// <param name="nameTable"></param>
        /// <returns></returns>

        public static AddIn Load(IAddInTree addInTree, string fileName, XmlNameTable nameTable = null)
        {
            try
            {
                using (TextReader textReader = File.OpenText(fileName))
                {
                    AddIn addIn = Load(addInTree, textReader, Path.GetDirectoryName(fileName), nameTable);
                    addIn.addInFileName = fileName;
                    return addIn;
                }
            }
            catch (AddInException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AddInException("Can't load " + fileName, e);
            }
        }

        /// <summary>
        /// 通过一个流来加载插件信息
        /// </summary>
        /// <param name="addInTree"></param>
        /// <param name="textReader"></param>
        /// <param name="hintPath"></param>
        /// <param name="nameTable"></param>
        /// <returns></returns>
        public static AddIn Load(IAddInTree addInTree, TextReader textReader, string hintPath = null, XmlNameTable nameTable = null)
        {
            if (nameTable == null)
                nameTable = new NameTable();
            try
            {
                AddIn addIn = new AddIn(addInTree);

                using (XmlTextReader reader = new XmlTextReader(textReader, nameTable))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.LocalName)
                            {
                                case "AddIn":
                                    addIn.Properties = AddInProperties.ReadFromAttributes(reader);
                                    SetupAddIn(reader, addIn, hintPath);
                                    break;
                                default:
                                    throw new AddInException("Unknown add-in file.");
                            }
                        }
                    }
                }
                return addIn;
            }
            catch (XmlException ex)
            {
                throw new AddInException(ex.Message, ex);
            }
        }

        /// <summary>
        /// 装载插件内容
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="addIn"></param>
        /// <param name="hintPath"></param>
        static void SetupAddIn(XmlReader reader, AddIn addIn, string hintPath)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.IsStartElement())
                {
                    switch (reader.LocalName)
                    {
                        case "StringResources":
                        case "BitmapResources":
                            if (reader.AttributeCount != 1)
                            {
                                throw new AddInException("BitmapResources requires ONE attribute.");
                            }

                            string filename = reader.GetAttribute("file");

                            if (reader.LocalName == "BitmapResources")
                            {
                                addIn.BitmapResources.Add(filename);
                            }
                            else
                            {
                                addIn.StringResources.Add(filename);
                            }
                            break;
                        case "Runtime":
                            if (!reader.IsEmptyElement)
                            {
                                addIn.runtimes.AddRange(Runtime.ReadSection(reader, addIn, hintPath));
                            }
                            break;
                        case "Include":
                            if (reader.AttributeCount != 1)
                            {
                                throw new AddInException("Include requires ONE attribute.");
                            }
                            if (!reader.IsEmptyElement)
                            {
                                throw new AddInException("Include nodes must be empty!");
                            }
                            if (hintPath == null)
                            {
                                throw new AddInException("Cannot use include nodes when hintPath was not specified (e.g. when AddInManager reads a .addin file)!");
                            }
                            string fileName = Path.Combine(hintPath, reader.GetAttribute(0));
                            XmlReaderSettings xrs = new XmlReaderSettings();
                            xrs.NameTable = reader.NameTable;
                            xrs.ConformanceLevel = ConformanceLevel.Fragment;
                            using (XmlReader includeReader = XmlTextReader.Create(fileName, xrs))
                            {
                                SetupAddIn(includeReader, addIn, Path.GetDirectoryName(fileName));
                            }
                            break;
                        case "Path":
                            if (reader.AttributeCount != 1)
                            {
                                throw new AddInException("Import node requires ONE attribute.");
                            }
                            string pathName = reader.GetAttribute(0);
                            AddInPath addInPath = addIn.GetExtensionPath(pathName);
                            if (!reader.IsEmptyElement)
                            {
                                AddInPath.SetUp(addInPath, reader, "Path");
                            }
                            break;
                        case "Manifest":
                            addIn.Manifest.ReadManifestSection(reader, hintPath);
                            break;
                        default:
                            throw new AddInException("Unknown root path node:" + reader.LocalName);
                    }
                }
            }
        }


    }
}
