namespace MondainDeploy
{
    public static class MondainUI
    {
        public static int TryParseWithDefault(string inputString, int defaultvalue)
        {
            int val;
            if (!int.TryParse(inputString, out val))
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

        public static string PrependStringTo(string secondString, string firstString)
        {
            return firstString + secondString;
        }

        public static string PrependLineTo(string secondString, string firstString)
        {
            return PrependStringTo(secondString, firstString + "<br />");
        }

        // todo: refine the behavior of this function. See unit test.
        public static string PostpendLineTo(string secondString, string firstString)
        {
            return PrependStringTo(firstString + "<br />", secondString);
        }

        public static string FormatMissedRightAnswer(string str)
        {
            return (Embolden(Italicize(str)));
        }
    }
}
