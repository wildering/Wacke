using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WackeClient.tool
{
    public static class StringHelper
    {
        public static string GetTextBetween(string text, string startKeyword, string endKeyword)
        {
            int startIndex = text.IndexOf(startKeyword) + startKeyword.Length;
            int endIndex = text.IndexOf(endKeyword, startIndex);
            string result = text.Substring(startIndex, endIndex - startIndex);
            return result;
        }


    }
}
