using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Doubts.Framework.EL.Compiler;

namespace Doubts.Framework.EL
{
    public class DefaultELAnalyzer : IELAnalyzer
    {
        private readonly ELCompiler elCompiler;

        public DefaultELAnalyzer()
        {
            this.elCompiler = new ELCompiler();
        }

        public Func<object, object> AnalyzeGetValue(string el)
        {
            return elCompiler.Compile(el);
        }

        public Action<object, object> AnalyzeSetValue(string el)
        {
            throw new NotImplementedException();
        }
    }
}
