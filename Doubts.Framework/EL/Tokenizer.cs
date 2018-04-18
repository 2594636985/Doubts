using Doubts.Framework.EL.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Doubts.Framework.EL
{
    internal class Tokenizer
    {
        public IEnumerable<Token> Tokenize(string source)
        {
            try
            {
                return Parse(source);
            }
            catch (Exception ex)
            {
                throw new ELException("An unhandled exception occurred while trying to compile the template", ex);
            }
        }

        private IEnumerable<Token> Parse(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new InvalidOperationException("the expression is invalid!! check please");

            int sIndex = 0;
            int eIndex = 0;

            while (sIndex < source.Length && eIndex < source.Length)
            {
                char node = source[eIndex++];

                if (node == '.')
                {
                    string property = source.Substring(sIndex, (eIndex - sIndex - 1));

                    sIndex = eIndex;

                    if (string.IsNullOrWhiteSpace(property))
                        throw new InvalidDataException("the property name is  empty! check please");

                    yield return new PropertyExpressionToken(property);
                }
                else if (node == '[' && (source[eIndex] == '\'' || source[eIndex] == '\"'))
                {
                    string property = source.Substring(sIndex, (eIndex - sIndex));

                    if (string.IsNullOrWhiteSpace(property))
                        throw new InvalidDataException("the property name is  empty! check please");

                    sIndex = ++eIndex;

                    while (eIndex < source.Length && source[eIndex] != '\'' && source[eIndex] != '\"')
                        eIndex++;

                    eIndex++;

                    if ((eIndex) > source.Length || source[eIndex] != ']')
                        throw new InvalidOperationException("the expression is wrong grammar! check please");

                    string indexes = source.Substring(sIndex, (eIndex - sIndex));

                    sIndex = eIndex;

                    if (string.IsNullOrWhiteSpace(indexes))
                        throw new InvalidDataException("the indexes name is  empty! check please");

                    yield return new PropertyExpressionToken(property, indexes);
                }
                else if (sIndex < eIndex && eIndex == source.Length - 1)
                {
                    string property = source.Substring(sIndex);

                    sIndex = eIndex++;

                    yield return new PropertyExpressionToken(property);
                }
            }
        }
    }
}
