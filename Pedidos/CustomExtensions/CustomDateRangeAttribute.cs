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
    sealed public class CustomDateRangeAttribute : ValidationAttribute, IClientValidatable
    {
        private DateTime _desde;
        private DateTime _hasta;

        public CustomDateRangeAttribute(string desde, string hasta, string mensajedeerror) : base(mensajedeerror)
        {
            _desde = DateTime.Parse(desde);
            _hasta = DateTime.Parse(hasta);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime valor = (DateTime)value;
            if (valor < _desde || valor > _hasta)
            {
                return new ValidationResult(validationContext.DisplayName);
            }
            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            string mensajedeerror = ErrorMessageString;

            var rule = new ModelClientValidationRule();
            rule.ErrorMessage = mensajedeerror;
            rule.ValidationType = "customdaterange";

            rule.ValidationParameters.Add("format", Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern);
            rule.ValidationParameters.Add("from", _desde.ToShortDateString());
            rule.ValidationParameters.Add("to", _hasta.ToShortDateString());
            yield return rule;
        }
    }
}