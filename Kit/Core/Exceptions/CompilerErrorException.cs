using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kit.Core.Exceptions
{
    class CompilerErrorException : Exception
    {
        public CompilerErrorException(string error)
            : base(error)
        { }
    }
}
