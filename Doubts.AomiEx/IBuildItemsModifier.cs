using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.AomiEx
{
    public interface IBuildItemsModifier
    {
        void Apply(IList items);
    }
}
