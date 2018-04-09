using Doubts.Framework.EL.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Doubts.Framework.EL.Parsers
{
    internal abstract class Parser
    {
        public abstract Token Parse(TextReader reader);
    }
}
