using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.AomiEx
{
    public interface IConditionEvaluator
    {
        bool IsValid(object parameter, Condition condition);
    }
}
