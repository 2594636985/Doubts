using Doubts.Framework.EL.Compiler;
using Doubts.Framework.EL.Compiler.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Doubts.Framework.EL
{
    public class ExpressionBuilder
    {
        public IEnumerable<Expression> ConvertTokensToExpressions(IEnumerable<object> tokens)
        {
            tokens = ElPropertyConverter.Convert(tokens);
            return tokens.Cast<Expression>();
        }
    }
}
