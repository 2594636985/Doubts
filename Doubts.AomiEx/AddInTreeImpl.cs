using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using Doubts.AomiEx.Properties;
using Doubts.AomiEx.DoozerImpl;

namespace Doubts.AomiEx
{
    public class AddInTreeImpl : IAddInTree
    {
        private AddInTreeNode rootNode = new AddInTreeNode();
        private List<AddIn> addIns = new List<AddIn>();
        private ConcurrentDictionary<string, IDoozer> doozers = new ConcurrentDictionary<string, IDoozer>();
        private ConcurrentDictionary<string, IConditionEvaluator> conditionEvaluators = new ConcurrentDictionary<string, IConditionEvaluator>();

        public ConcurrentDictionary<string, IConditionEvaluator> ConditionEvaluators
        {
            get
            {
                return conditionEvaluators;
            }
        }

        public ReadOnlyCollection<AddIn> AddIns
        {
            get
            {
                return addIns.AsReadOnly();
            }
        }

        public ConcurrentDictionary<string, IDoozer> Doozers
        {
            get
            {
                return doozers;
            }
        }

        public AddInTreeImpl()
        {
            this.doozers.TryAdd("Class", new ClassDoozer());

            //conditionEvaluators.TryAdd("Compare", new CompareConditionEvaluator());
            //conditionEvaluators.TryAdd("Ownerstate", new OwnerStateConditionEvaluator());
        }

        public AddInTreeNode GetTreeNode(string path, bool throwOnNotFound = true)
        {
            if (path == null || path.Length == 0)
            {
                return rootNode;
            }
            string[] splittedPath = path.Split('/');
            AddInTreeNode curPath = rootNode;
            for (int i = 0; i < splittedPath.Length; i++)
            {
                if (!curPath.ChildNodes.TryGetValue(splittedPath[i].ToUpper(), out curPath))
                {
                    if (throwOnNotFound)
                        throw new AddInException(string.Format(StringResources.AddIn_TreePath_NotFound, path));
                    else
                        return null;
                }
            }
            return curPath;
        }


        public void InsertAddIn(AddIn addIn)
        {
            if (addIn.Enabled)
            {
                foreach (AddInPath path in addIn.Paths.Values)
                {
                    AddExtensionPath(path);
                }

                foreach (Runtime runtime in addIn.Runtimes)
                {
                    if (runtime.IsActive)
                    {
                        foreach (var pair in runtime.DefinedDoozers)
                        {
                            if (!doozers.TryAdd(pair.Key, pair.Value))
                                throw new AddInException("Duplicate doozer: " + pair.Key);
                        }
                        foreach (var pair in runtime.DefinedConditionEvaluators)
                        {
                            if (!conditionEvaluators.TryAdd(pair.Key, pair.Value))
                                throw new AddInException("Duplicate condition evaluator: " + pair.Key);
                        }
                    }
                }

                string addInRoot = Path.GetDirectoryName(addIn.FileName);

                foreach (string bitmapResource in addIn.BitmapResources)
                {
                    string path = Path.Combine(addInRoot, bitmapResource);
                    ResourceManager resourceManager = ResourceManager.CreateFileBasedResourceManager(Path.GetFileNameWithoutExtension(path), Path.GetDirectoryName(path), null);
                    //ServiceSingleton.GetRequiredService<IResourceService>().RegisterNeutralImages(resourceManager);
                }

                foreach (string stringResource in addIn.StringResources)
                {
                    string path = Path.Combine(addInRoot, stringResource);
                    ResourceManager resourceManager = ResourceManager.CreateFileBasedResourceManager(Path.GetFileNameWithoutExtension(path), Path.GetDirectoryName(path), null);
                    //ServiceSingleton.GetRequiredService<IResourceService>().RegisterNeutralStrings(resourceManager);
                }
            }
            addIns.Add(addIn);
        }

        private void AddExtensionPath(AddInPath path)
        {
            AddInTreeNode treePath = CreatePath(rootNode, path.Name);

            foreach (IEnumerable<Codon> innerCodons in path.GroupedCodons)
                treePath.AddCodons(innerCodons);
        }

        AddInTreeNode CreatePath(AddInTreeNode localRoot, string path)
        {
            if (path == null || path.Length == 0)
            {
                return localRoot;
            }

            string[] splittedPath = path.Split('/');

            AddInTreeNode curPath = localRoot;

            int i = 0;

            while (i < splittedPath.Length)
            {
                string keyValue = splittedPath[i]?.ToUpper();

                if (!curPath.ChildNodes.ContainsKey(keyValue))
                {
                    curPath.ChildNodes[keyValue] = new AddInTreeNode();
                }

                curPath = curPath.ChildNodes[keyValue];

                ++i;
            }

            return curPath;
        }

        private void DisableAddin(AddIn addIn, Dictionary<string, Version> dict, Dictionary<string, AddIn> addInDict)
        {
            addIn.Enabled = false;
            addIn.Action = AddInAction.DependencyError;

            foreach (string name in addIn.Manifest.Identities.Keys)
            {
                dict.Remove(name);
                addInDict.Remove(name);
            }

        }

        /// <summary>
        /// 加载插件树
        /// </summary>
        /// <param name="addInFiles"></param>
        /// <param name="disabledAddIns"></param>
        public void Load(List<string> addInFiles, List<string> disabledAddIns)
        {
            List<AddIn> list = new List<AddIn>();
            Dictionary<string, Version> dict = new Dictionary<string, Version>();
            Dictionary<string, AddIn> addInDict = new Dictionary<string, AddIn>();

            var nameTable = new System.Xml.NameTable();

            foreach (string fileName in addInFiles)
            {
                AddIn addIn;

                try
                {
                    addIn = AddIn.Load(this, fileName, nameTable);
                }
                catch (AddInException ex)
                {
                    addIn = new AddIn(this);
                    addIn.addInFileName = fileName;
                    addIn.CustomErrorMessage = ex.Message;
                }

                if (addIn.Action == AddInAction.CustomError)
                {
                    list.Add(addIn);
                    continue;
                }

                addIn.Enabled = true;

                if (disabledAddIns != null && disabledAddIns.Count > 0)
                {
                    foreach (string name in addIn.Manifest.Identities.Keys)
                    {
                        if (disabledAddIns.Contains(name))
                        {
                            addIn.Enabled = false;
                            break;
                        }
                    }
                }

                if (addIn.Enabled)
                {
                    foreach (KeyValuePair<string, Version> pair in addIn.Manifest.Identities)
                    {
                        if (dict.ContainsKey(pair.Key))
                        {
                            addIn.Enabled = false;
                            addIn.Action = AddInAction.InstalledTwice;
                            break;
                        }
                        else
                        {
                            dict.Add(pair.Key, pair.Value);
                            addInDict.Add(pair.Key, addIn);
                        }
                    }
                }
                list.Add(addIn);
            }

            checkDependencies:

            for (int i = 0; i < list.Count; i++)
            {
                AddIn addIn = list[i];

                if (!addIn.Enabled)
                    continue;

                Version versionFound;

                foreach (AddInReference reference in addIn.Manifest.Conflicts)
                {
                    if (reference.Check(dict, out versionFound))
                    {
                        DisableAddin(addIn, dict, addInDict);

                        goto checkDependencies;
                    }
                }
                foreach (AddInReference reference in addIn.Manifest.Dependencies)
                {
                    if (!reference.Check(dict, out versionFound))
                    {
                        DisableAddin(addIn, dict, addInDict);
                        goto checkDependencies;
                    }
                }
            }

            foreach (AddIn addIn in list)
            {
                try
                {
                    InsertAddIn(addIn);
                }
                catch (AddInException ex)
                {
                }
            }

        }
    }
}
