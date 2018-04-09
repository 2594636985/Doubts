using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.Framework.EL
{
    public class ELException : DoubtsException
    {
        public ELException(string message) : base(message)
        {

        }

        public ELException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
