using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.Framework.EL
{
    public interface IELAnalyzer
    {
        Func<object, object> AnalyzeGetValue(string el);

        Action<object, object> AnalyzeSetValue(string el);

    }
}
