using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doubts.Framework
{
    public class DoubtsException : Exception
    {
        public DoubtsException(string message) : base(message)
        {
        }

        public DoubtsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
