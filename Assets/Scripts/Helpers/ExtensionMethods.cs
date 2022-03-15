using System.Text.RegularExpressions;

namespace Helpers
{
    public static class ExtensionMethods
    {
        public static int SetLength(this int _num, int _newLength, int _fill)
        {
            string _newNumStr = _num.ToString();
            for (int i = _num.ToString().Length; i <= _newLength; i++)
            {
                _newNumStr += _fill.ToString();
            }

            return int.Parse(_newNumStr);
        }

        public static string GetDigitsToString(this string _str)
        {
            return Regex.Match(_str, @"\d+").Value;
        }
        
        public static int GetDigits(this string _str)
        {
            return int.Parse(Regex.Match(_str, @"\d+").Value);
        }
    }
}