using CustomExtensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pedidos.ViewModels
{
    public class GestionesAltaVM
    {
        [CustomDate]
        [Required]
        [Display(Name = "Fecha")]
        public System.DateTime FechaGestion { get; set; }

        [CustomClienteId(CustomClienteIdAttribute.Options.shouldExist, false)]
        [MaxLength(15)]
        [Display(Name = "Cliente")]
        public string ClienteId { get; set; }

        [DataType(DataType.MultilineText)]
        public string Observaciones { get; set; }

        public string VolverA { get; set; }
    }
}