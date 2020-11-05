#region

using System;
using System.Globalization;

#endregion

namespace RIS.Core.Helper
{
    public class Parser
    {
        /// <summary>
        ///     Try to parse string in double else return 0
        /// </summary>
        /// <param name="valueString"></param>
        public static double StringToDouble(string valueString)
        {
            if (string.IsNullOrWhiteSpace(valueString)) return 0;

            valueString = valueString.Replace(".", ",");
            double valueDouble = 0;
            double.TryParse(valueString,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands |
                NumberStyles.Float, null, out valueDouble);

            return Math.Round(valueDouble, 2);
        }

        /// <summary>
        ///     Try to parse string in int else return 0
        /// </summary>
        /// <param name="valueString"></param>
        public static int StringToInt(string valueString)
        {
            var valueInt = 0;
            int.TryParse(valueString, out valueInt);

            return valueInt;
        }
    }
}