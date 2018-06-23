using System;
using System.ComponentModel.DataAnnotations;

namespace CustomExtensions
{
    [AttributeUsage(AttributeTargets.Property |
      AttributeTargets.Field, AllowMultiple = false)]
    sealed public class CustomClienteIdAttribute : ValidationAttribute
    {
        public enum Options
        {
            shouldExist = 0,
            shouldNotExist = 1
        }

        private Options _options = Options.shouldExist;
        private bool _allowNull;

        public CustomClienteIdAttribute(Options options, bool allowNull) {
            _options = options;
            _allowNull = allowNull;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Si se admite el campo con un valor nulo y el campo efectivamente contiene un valor nulo, la validación es exitosa.
            if (_allowNull && value == null) return ValidationResult.Success;

            // Si no se admite que el valor del campo sea nulo y el campo lo es, la validación falla.
            if (!_allowNull && value == null) return new ValidationResult("El campo " + validationContext.DisplayName + " no puede estar vacío.");

            // Se ha especificado un valor en el campo. Se analiza su validez consultando la base de datos.
            using (var db = new Pedidos.DAL.PedidosDbContext())
            {
                var cliente = db.Clientes.Find(value);
                if ((cliente == null && _options == Options.shouldExist) || ((cliente != null && _options == Options.shouldNotExist)))
                {
                    return new ValidationResult("El campo " + validationContext.DisplayName + " no contiene un código de cliente válido.");
                }
                return ValidationResult.Success;
            }
        }

    }
}