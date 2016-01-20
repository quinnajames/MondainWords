using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MondainDeploy
{ 
    public static class MondainUI
    {
        public static int TryParseWithDefault(string input_string, int defaultvalue)
        {
            int val;
            if (!Int32.TryParse(input_string, out val))
                val = defaultvalue;
            return val;
        }
    }
}
