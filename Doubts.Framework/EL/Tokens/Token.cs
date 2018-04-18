using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.Framework.EL.Tokens
{
    internal abstract class Token
    {
        public abstract TokenType Type { get; }

        public static PropertyExpressionToken Property(string property)
        {
            return new PropertyExpressionToken(property);
        }
    }
}
