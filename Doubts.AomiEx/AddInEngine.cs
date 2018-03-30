using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;

namespace Doubts.AomiEx
{
    internal abstract class AddInEngine : IAddInEngine
    {
        private List<string> addInFiles = new List<string>();
        private List<string> disabledAddIns = new List<string>();
        private bool externalAddInsConfigured;
        private AddInTreeImpl addInTree;

        public List<string> DisableAddIns
        {
            get { return disabledAddIns; }
        }

        public List<string> AddInFiles
        {
            get { return this.addInFiles; }
        }

        public AddInTreeImpl AddInTree
        {
            get { return addInTree; }
        }

        public AddInEngine()
        {
            this.addInTree = new AddInTreeImpl();
        }

        public virtual void AddAddInsFromDirectory(string addInDir)
        {
            if (addInDir == null)
                throw new ArgumentNullException("addInDir");

            addInFiles.AddRange(Directory.GetFiles(addInDir, "*.addin", SearchOption.AllDirectories));
        }

        public virtual void AddAddInFile(string addInFile)
        {
            if (addInFile == null)
                throw new ArgumentNullException("addInFile");

            addInFiles.Add(addInFile);
        }

        public virtual void Initialize()
        {


        }

        public object BuildItem(string path, object parameter)
        {
            return BuildItem(path, parameter, null);
        }

        public object BuildItem(string path, object parameter, IEnumerable<ICondition> additionalConditions)
        {
            int pos = path.LastIndexOf('/');
            string parent = path.Substring(0, pos);
            string child = path.Substring(pos + 1);
            AddInTreeNode node = this.addInTree.GetTreeNode(parent);

            return node.BuildChildItem(child, parameter, additionalConditions);
        }

        public List<T> BuildItems<T>(string path, object parameter, bool throwOnNotFound = true)
        {
            AddInTreeNode node = this.addInTree.GetTreeNode(path, throwOnNotFound);
            if (node == null)
                return new List<T>();
            else
                return node.BuildChildItems<T>(parameter);
        }

    
    }
}
