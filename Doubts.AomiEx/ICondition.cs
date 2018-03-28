using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.AomiEx
{
    public interface ICondition
    {
        string Name
        {
            get;
        }
   
        ConditionFailedAction Action
        {
            get;
            set;
        }

        bool IsValid(object parameter);
    }
}
