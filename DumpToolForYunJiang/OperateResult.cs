using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DumpToolForYunJiang
{
    class ButtonResult
    {
        public enum ConvertResult
        {
            Success,
            FailNoReason,
            
        }
        public enum SaveResult
        {
            Success,
            FailNoReason,
            FailNotConvert
        }
    }
    
}
