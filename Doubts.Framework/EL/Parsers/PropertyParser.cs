using Doubts.Framework.EL.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Doubts.Framework.EL.Parsers
{
    internal class PropertyParser : Parser
    {
        private const string validWordStartCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_";

        public override Token Parse(TextReader reader)
        {
            if (IsPropertyCharacters(reader))
            {
                return Token.Property(AccumulateProperty(reader));
            }
            return null;
        }

        private bool IsPropertyCharacters(TextReader reader)
        {
            char peek = (char)reader.Peek();

            return validWordStartCharacters.Contains(peek.ToString());
        }

        private string AccumulateProperty(TextReader reader)
        {
            StringBuilder buffer = new StringBuilder();

            while (true)
            {
                int data = reader.Read();

                if (data == -1)
                {
                    throw new InvalidOperationException("Reached end of template before the expression was closed.");
                }

                char node = (char)data;

                if (node == '.' || Char.IsWhiteSpace((char)node))
                {
                    break;
                }

                buffer.Append((char)node);
            }

            return buffer.ToString().Trim();
        }
    }
}
