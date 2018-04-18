using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.Framework.EL.Tokens
{
    internal class PropertyExpressionToken : ExpressionToken
    {
        private readonly string propertyName;
        private readonly string indexes;
        public PropertyExpressionToken(string propertyName) : this(propertyName, null)
        {
        }
        public PropertyExpressionToken(string propertyName, string indexes)
        {
            this.propertyName = propertyName;
            this.indexes = indexes;
        }

        public string PropertyName { get { return this.propertyName; } }

        public string Indexes { get { return this.indexes; } }

        public override TokenType Type
        {
            get { return TokenType.Property; }
        }
    }
}
