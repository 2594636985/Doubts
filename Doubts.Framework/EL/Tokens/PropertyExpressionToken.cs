using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.Framework.EL.Tokens
{
    internal class PropertyExpressionToken : ExpressionToken
    {
        private readonly string property;

        public PropertyExpressionToken(string property)
        {
            this.property = property;
        }

        public override TokenType Type
        {
            get { return TokenType.Property; }
        }

        public override string Value
        {
            get { return property; }
        }
    }
}
