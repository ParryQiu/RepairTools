using System;
using System.Collections.Generic;
using System.Text;

namespace RepairTools_2._0
{
    class stringChecker
    {
        public static string stringReplacer(string str)
        {
            string newString = str.Replace("/min",null);
            newString = newString.Replace("/start",null);
            newString = newString.Replace("\"", null);
            newString = newString.Replace("-helper", null);
            newString = newString.Replace("rundll32", null);
            return newString;
        }
    }
}
