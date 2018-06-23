using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Specialized;

namespace CustomExtensions
{
    // Versión 1.0 05/08/2015 16:24
    public static class ActionLinkExtensionMethods
    {
        
        // Versión 1.0 05/08/2015 16:24
        public static MvcHtmlString ActionLinkwParams(this HtmlHelper helper, string linktext, string action, string controller, object extraRVs, object htmlAttributes)
        {

            RouteValueDictionary _QueryStringRVs = helper.ViewContext.RequestContext.HttpContext.Request.QueryString.toRouteValueDictionary();

            RouteValueDictionary _htmlAttributes = new RouteValueDictionary(htmlAttributes);

            RouteValueDictionary _extraRVs;
            if (extraRVs is RouteValueDictionary)
            {
                _extraRVs = (RouteValueDictionary)extraRVs;
            }
            else
	        {
                _extraRVs = new RouteValueDictionary(extraRVs);
	        }

            RouteValueDictionary _RVs = Merge(_QueryStringRVs, _extraRVs);

            return System.Web.Mvc.Html.LinkExtensions.ActionLink(helper, linktext, action, controller, _RVs, _htmlAttributes);
        }

        // Versión 1.0 04/08/2015 00:00
        public static MvcHtmlString ActionLinkwParams(this HtmlHelper helper, string linktext, string action)
        {
            return ActionLinkwParams(helper, linktext, action, null, null, null);
        }

        // Versión 1.0 04/08/2015 00:00
        public static MvcHtmlString ActionLinkwParams(this HtmlHelper helper, string linktext, string action, string controller)
        {
            return ActionLinkwParams(helper, linktext, action, controller, null, null);
        }

        // Versión 1.0 04/08/2015 00:00
        public static MvcHtmlString ActionLinkwParams(this HtmlHelper helper, string linktext, string action, object extraRVs)
        {
            return ActionLinkwParams(helper, linktext, action, null, extraRVs, null);
        }

        // Versión 1.0 04/08/2015 00:00
        public static MvcHtmlString ActionLinkwParams(this HtmlHelper helper, string linktext, string action, string controller, object extraRVs)
        {
            return ActionLinkwParams(helper, linktext, action, controller, extraRVs, null);
        }

        // Versión 1.0 04/08/2015 00:00
        public static MvcHtmlString ActionLinkwParams(this HtmlHelper helper, string linktext, string action, object extraRVs, object htmlAttributes)
        {
            return ActionLinkwParams(helper, linktext, action, null, extraRVs, htmlAttributes);
        }

        // Versión 1.0 05/08/2015 16:24
        public static RouteValueDictionary toRouteValueDictionary(this NameValueCollection qs)
        {

            RouteValueDictionary output = new RouteValueDictionary();
            foreach (string _key in qs.AllKeys)
            {
                output.Add(_key, qs[_key]);
            }
            return output;
        }

        // Versión 1.0 04/08/2015 00:00
        public static RouteValueDictionary Merge(this RouteValueDictionary original, RouteValueDictionary @new)
        {

            // Create a new dictionary containing implicit and auto-generated values
            RouteValueDictionary merged = new RouteValueDictionary(original);

            foreach (var f in @new)
            {
                if (merged.ContainsKey(f.Key))
                {
                    merged[f.Key] = f.Value;
                }
                else
                {
                    merged.Add(f.Key, f.Value);
                }
            }

            return merged;
        }
    }
}