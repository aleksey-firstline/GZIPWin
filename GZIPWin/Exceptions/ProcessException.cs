using System;
using System.Collections.Generic;
using System.Text;

namespace GZIPWin.Exceptions
{
    [Serializable]
    public class ProcessException : Exception
    {
        public ProcessException() : 
            base("Invalid processing file")
        {
            
        }
    }
}
