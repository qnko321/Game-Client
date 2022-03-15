using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    public static class LogHelper
    {
        public static void LogDictionary<T, T2>(Dictionary<T, T2> _dict)
        {
            string _log = "{ ";
            foreach (KeyValuePair<T,T2> _pair in _dict)
            {
                _log += $"[{_pair.Key}: {_pair.Value}], ";
            }
            _log += " }";
            Debug.Log(_log);
        }
    }
}