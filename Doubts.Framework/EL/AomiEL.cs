using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Doubts.Framework.EL
{
    public sealed class AomiEL
    {
        private static readonly Lazy<IELAnalyzer> lazyIELAnalyzer = new Lazy<IELAnalyzer>(() => new DefaultELAnalyzer());

        private static IELAnalyzer ELAnalyzer { get { return lazyIELAnalyzer.Value; } }

        public static object GetValue(object instance, string el)
        {
            Func<object, object> func = ELAnalyzer.AnalyzeGetValue(el);

            return func.Invoke(instance);
        }

        public static void SetValue(object instance, string el, object value)
        {
            Action<object, object> action = ELAnalyzer.AnalyzeSetValue(el);

            action.Invoke(instance, value);
        }
    }
}
