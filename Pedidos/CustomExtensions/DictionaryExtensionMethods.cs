using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomExtensions
{
    // Versión 2.0 05/08/2015 12:19
    public static class DictionaryExtensionMethods
    {
        public static TKey[] FilterKeys<TKey, TValue>(this Dictionary<TKey, TValue> dic, string searchInValues) {
            var keys = from kvP in dic
                    where kvP.Value.ToString().IndexOf(searchInValues, StringComparison.OrdinalIgnoreCase) != -1
                    // any value that isn't `-1` means it contains the text
                    // (or the description was empty)
                    select kvP.Key;
            return keys.ToArray();

        }
    }
}