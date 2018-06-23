using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Threading;

namespace CustomExtensions
{
    [AttributeUsage(AttributeTargets.Property |
     AttributeTargets.Field, AllowMultiple = false)]
    sealed public class CustomDateAttribute : ValidationAttribute, IClientValidatable
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                if (((Nullable<DateTime>)value).HasValue)
                {
                    if (((Nullable<DateTime>)value).Value < new DateTime(1751, 1, 1))
                    {
                        return null;
                    }
                }
            }
                return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            string mensajedeerror = ErrorMessageString;

            var rule = new ModelClientValidationRule();
            rule.ErrorMessage = mensajedeerror;
            rule.ValidationType = "customdate";
            rule.ValidationParameters.Add("format", Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern);
            yield return rule;
        }
    }
}