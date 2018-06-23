using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Expressions.Internal;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace CustomExtensions
{
    public static class HtmlHelpers
    {
        public static string DisplayShortNameFor<TModel, TValue>(this HtmlHelper<TModel> t, Expression<Func<TModel,TValue>> exp)
        {
            CustomAttributeNamedArgument? DisplayName = null;
            MemberExpression prop = exp.Body as MemberExpression;
            if (prop != null)
            {
                CustomAttributeData DisplayAttrib = (
                                    from c in prop.Member.GetCustomAttributesData()
                                    where c.AttributeType == typeof(DisplayAttribute)
                                    select c).FirstOrDefault();
                if (DisplayAttrib != null)
                { 
                    DisplayName = DisplayAttrib.NamedArguments.Where(d => d.MemberName == "ShortName").FirstOrDefault();
                    if (DisplayName.Value.TypedValue.Value == null)
                    {
                        DisplayName = DisplayAttrib.NamedArguments.Where(d => d.MemberName == "Name").FirstOrDefault();
                    }
                }
                return (DisplayAttrib != null) ? ((DisplayName.Value.TypedValue.Value != null) ? DisplayName.Value.TypedValue.Value.ToString() : "") : prop.Member.Name;
            }
            else
            {
                return "";
            }
        }

        public static string DisplayShortFor<TModel>(this HtmlHelper<TModel> html, string text, int lenght)
        {
            text = text ?? "";
            if (text.Length > lenght)
            {
                return text.Substring(0, lenght) + @"...";
            }
            else
            {
                return text;
            }
        }
    }
}