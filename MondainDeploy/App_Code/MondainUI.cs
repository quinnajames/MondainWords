using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MondainDeploy
{ 
    public static class MondainUI
    {
        public static int TryParseWithDefault(string inputString, int defaultvalue)
        {
            int val;
            if (!Int32.TryParse(inputString, out val))
                val = defaultvalue;
            return val;
        }
    }
}
