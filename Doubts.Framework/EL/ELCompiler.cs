using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Doubts.Framework.EL
{
    public class ELCompiler
    {
        private Tokenizer tokenizer;
        private FunctionBuilder functionBuilder;
        private ExpressionBuilder expressionBuilder;

        public ELCompiler()
        {
            expressionBuilder = new ExpressionBuilder();
            functionBuilder = new FunctionBuilder();
        }

        public Action<TextWriter, object> Compile(TextReader source)
        {
            var tokens = tokenizer.Tokenize(source).ToList();
            var expressions = expressionBuilder.ConvertTokensToExpressions(tokens);
            return functionBuilder.Compile(expressions);
        }
    }
}
