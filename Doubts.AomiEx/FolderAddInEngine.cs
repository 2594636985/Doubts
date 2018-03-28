using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.AomiEx
{
    internal class FolderAddInEngine : AddInEngine
    {
        public override void Initialize()
        {
            this.AddInTree.Load(this.AddInFiles, this.DisableAddIns);
        }
    }
}
