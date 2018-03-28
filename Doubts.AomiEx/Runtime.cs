using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Doubts.AomiEx.DoozerImpl;
using Doubts.AomiEx.ConditionEvaluatorImpl;
using System.IO;

namespace Doubts.AomiEx
{
    public class Runtime
    {
        private string hintPath;
        private string assembly;
        private Assembly loadedAssembly = null;

        private List<LazyLoadDoozer> definedDoozers = new List<LazyLoadDoozer>();
        private List<LazyConditionEvaluator> definedConditionEvaluators = new List<LazyConditionEvaluator>();
        private ICondition[] conditions;
        private IAddInTree addInTree;
        private bool isActive = true;
        private bool isAssemblyLoaded;
        private readonly object lockObj = new object(); // used to protect mutable parts of runtime

        public bool IsActive
        {
            get
            {
                lock (lockObj)
                {
                    if (conditions != null)
                    {
                        isActive = Condition.GetFailedAction(conditions, this) == ConditionFailedAction.Nothing;
                        conditions = null;
                    }
                    return isActive;
                }
            }
        }

        public Runtime(IAddInTree addInTree, string assembly, string hintPath)
        {
            if (addInTree == null)
                throw new ArgumentNullException("addInTree");
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            this.addInTree = addInTree;
            this.assembly = assembly;
            this.hintPath = hintPath;
        }

        public string Assembly
        {
            get { return assembly; }
        }

        /// <summary>
        /// Gets whether the assembly belongs to the host application (':' prefix).
        /// </summary>
        public bool IsHostApplicationAssembly
        {
            get { return !string.IsNullOrEmpty(assembly) && assembly[0] == ':'; }
        }

        /// <summary>
        /// Force loading the runtime assembly now.
        /// </summary>
        public void Load()
        {
            lock (lockObj)
            {
                if (!isAssemblyLoaded)
                {
                    if (!this.IsActive)
                        throw new InvalidOperationException("Cannot load inactive AddIn runtime");

                    isAssemblyLoaded = true;

                    try
                    {
                        if (assembly[0] == ':')
                        {
                            loadedAssembly = LoadAssembly(assembly.Substring(1));
                        }
                        else if (assembly[0] == '$')
                        {
                            //$ICSharpCode.TextTemplating/TextTemplating.dll

                            int pos = assembly.IndexOf('/');
                            if (pos < 0)
                                throw new AddInException("Expected '/' in path beginning with '$'!");
                            string referencedAddIn = assembly.Substring(1, pos - 1);
                            foreach (var addIn in addInTree.AddIns)
                            {
                                if (addIn.Enabled && addIn.Manifest.Identities.ContainsKey(referencedAddIn))
                                {
                                    string assemblyFile = Path.Combine(Path.GetDirectoryName(addIn.FileName),
                                                                       assembly.Substring(pos + 1));
                                    loadedAssembly = LoadAssemblyFrom(assemblyFile);
                                    break;
                                }
                            }
                            if (loadedAssembly == null)
                            {
                                throw new FileNotFoundException("Could not find referenced AddIn " + referencedAddIn);
                            }
                        }
                        else
                        {
                            loadedAssembly = LoadAssemblyFrom(Path.Combine(hintPath, assembly));
                        }

#if DEBUG
                        // preload assembly to provoke FileLoadException if dependencies are missing
                        loadedAssembly.GetExportedTypes();
#endif
                    }
                    catch (FileNotFoundException ex)
                    {
                        ShowError("The addin '" + assembly + "' could not be loaded:\n" + ex.ToString());
                    }
                    catch (FileLoadException ex)
                    {
                        ShowError("The addin '" + assembly + "' could not be loaded:\n" + ex.ToString());
                    }
                }
            }
        }

        public Assembly LoadedAssembly
        {
            get
            {
                if (this.IsActive)
                {
                    Load(); // load the assembly, if not already done
                    return loadedAssembly;
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<KeyValuePair<string, IDoozer>> DefinedDoozers
        {
            get
            {
                return definedDoozers.Select(d => new KeyValuePair<string, IDoozer>(d.Name, d));
            }
        }

        public IEnumerable<KeyValuePair<string, IConditionEvaluator>> DefinedConditionEvaluators
        {
            get
            {
                return definedConditionEvaluators.Select(c => new KeyValuePair<string, IConditionEvaluator>(c.Name, c));
            }
        }

        public Type FindType(string className)
        {
            Assembly asm = LoadedAssembly;
            if (asm == null)
                return null;
            return asm.GetType(className);
        }

        internal static List<Runtime> ReadSection(XmlReader reader, AddIn addIn, string hintPath)
        {
            List<Runtime> runtimes = new List<Runtime>();
            Stack<ICondition> conditionStack = new Stack<ICondition>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == "Condition" || reader.LocalName == "ComplexCondition")
                        {
                            conditionStack.Pop();
                        }
                        else if (reader.LocalName == "Runtime")
                        {
                            return runtimes;
                        }
                        break;
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "Condition":
                                conditionStack.Push(Condition.Read(reader, addIn));
                                break;
                            case "ComplexCondition":
                                conditionStack.Push(Condition.ReadComplexCondition(reader, addIn));
                                break;
                            case "Import":
                                runtimes.Add(Runtime.Read(addIn, reader, hintPath, conditionStack));
                                break;
                            case "DisableAddIn":
                                if (Condition.GetFailedAction(conditionStack, addIn) == ConditionFailedAction.Nothing)
                                {
                                    // The DisableAddIn node not was not disabled by a condition
                                    addIn.CustomErrorMessage = reader.GetAttribute("message");
                                }
                                break;
                            default:
                                throw new AddInException("Unknown node in runtime section :" + reader.LocalName);
                        }
                        break;
                }
            }
            return runtimes;
        }

        internal static Runtime Read(AddIn addIn, XmlReader reader, string hintPath, Stack<ICondition> conditionStack)
        {
            if (reader.AttributeCount != 1)
            {
                throw new AddInException("Import node requires ONE attribute.");
            }
            Runtime runtime = new Runtime(addIn.AddInTree, reader.GetAttribute(0), hintPath);
            if (conditionStack.Count > 0)
            {
                runtime.conditions = conditionStack.ToArray();
            }
            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.EndElement:
                            if (reader.LocalName == "Import")
                            {
                                return runtime;
                            }
                            break;
                        case XmlNodeType.Element:
                            string nodeName = reader.LocalName;
                            AddInProperties properties = AddInProperties.ReadFromAttributes(reader);
                            switch (nodeName)
                            {
                                case "Doozer":
                                    if (!reader.IsEmptyElement)
                                    {
                                        throw new AddInException("Doozer nodes must be empty!");
                                    }
                                    runtime.definedDoozers.Add(new LazyLoadDoozer(addIn, properties));
                                    break;
                                case "ConditionEvaluator":
                                    if (!reader.IsEmptyElement)
                                    {
                                        throw new AddInException("ConditionEvaluator nodes must be empty!");
                                    }
                                    runtime.definedConditionEvaluators.Add(new LazyConditionEvaluator(addIn, properties));
                                    break;
                                default:
                                    throw new AddInException("Unknown node in Import section:" + nodeName);
                            }
                            break;
                    }
                }
            }
            return runtime;
        }

        protected virtual Assembly LoadAssembly(string assemblyString)
        {
            return System.Reflection.Assembly.Load(assemblyString);
        }

        protected virtual Assembly LoadAssemblyFrom(string assemblyFile)
        {
            return System.Reflection.Assembly.LoadFrom(assemblyFile);
        }

        protected virtual void ShowError(string message)
        {
           
        }
    }
}
