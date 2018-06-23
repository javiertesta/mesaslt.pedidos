using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace CustomExtensions
{
    // Versión 2.0 05/08/2015 12:47
    public static class EnumExtensionMethods
    {
        // Versión 2.0 05/08/2015 12:47
        public static string GetEnumMemberDisplayName<TEnum>(this Enum enumeration)
        {
            if (enumeration != null)
            {
                string name = enumeration.ToString();
                return GetEnumMemberDisplayName<TEnum>(name);
            }
            return "";
        }

        // Versión 2.0 05/08/2015 12:47
        public static string GetEnumMemberDisplayName<TEnum>(string name)
        {
            Type type = typeof(TEnum);
            MemberInfo[] memInfo = type.GetMember(name);

            if (null != memInfo && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                if (null != attrs && attrs.Length > 0) return ((DisplayAttribute)attrs[0]).Name;
            }
            return name;
        }

        // Versión 2.0 05/08/2015 12:47
        public static Dictionary<TEnum, string> EnumDisplayNamesToDictionary<TEnum>()
        {
            Type type = typeof(TEnum);
            var output = new Dictionary<TEnum, string>();

            if (type.IsEnum)
            {
                IEnumerable<TEnum> values = Enum.GetValues(type).Cast<TEnum>();
                foreach (TEnum value in values)
                {
                    output.Add(value, GetEnumMemberDisplayName<TEnum>(value.ToString()));
                }
            }
            return output;
        }

    }
}