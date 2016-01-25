﻿using System;
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

        public static string Italicize(string str)
        {
            return "<i>" + str + "</i>";
        }

        public static string Embolden(string str)
        {
            return "<strong>" + str + "</strong>";
        }

        public static string Strike(string str)
        {
            return "<span style=\"color:lightgray\"><del>" + str + "</del></span>";
        }

        public static string PrependString(string inputString, string toBePrepended)
        {
            return toBePrepended + inputString;
        }

        public static string PrependLine(string inputString, string toBePrepended)
        {
            return PrependString(inputString, toBePrepended + "<br />");
        }

        public static string PostpendLine(string inputString, string toBePostpended)
        {
            return PrependString(toBePostpended + "<br />", inputString);
        } 
    }
}
