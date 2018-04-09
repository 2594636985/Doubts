using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Doubts.Framework.EL
{
    public class DefaultELAnalyzer : IELAnalyzer
    {
        private readonly ELCompiler elCompiler;

        public Action<TextWriter, object> Compile(TextReader elReader)
        {
            return elCompiler.Compile(elReader);
        }

        public Func<object, object> AnalyzeGetValue(string el)
        {
            using (StringReader elReader = new StringReader(el))
            {
                Action<TextWriter, object> compiledTemplate = Compile(elReader);

                return context =>
                {
                    StringBuilder builder = new StringBuilder();

                    using (StringWriter writer = new StringWriter(builder))
                    {
                        compiledTemplate(writer, context);
                    }

                    return builder.ToString();
                };
            }
        }

        public Action<object, object> AnalyzeSetValue(string el)
        {
            throw new NotImplementedException();
        }
    }
}
