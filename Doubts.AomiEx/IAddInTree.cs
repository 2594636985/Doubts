using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Doubts.AomiEx
{
    public interface IAddInTree
    {
        ConcurrentDictionary<string, IDoozer> Doozers { get; }

        ReadOnlyCollection<AddIn> AddIns { get; }

        ConcurrentDictionary<string, IConditionEvaluator> ConditionEvaluators { get; }

        void Load(List<string> addInFiles, List<string> disabledAddIns);
    }
}
