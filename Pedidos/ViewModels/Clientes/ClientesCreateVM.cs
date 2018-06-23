using CustomExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pedidos.ViewModels
{
    public class ClientesCreateVM
    {
        [CustomClienteId(CustomClienteIdAttribute.Options.shouldNotExist, false)]
        [MaxLength(15)]
        [Display(Name = "Código")]
        public string ClienteId { get; set; }

        [Display(Name = "Zona Geográfica")]
        public Pedidos.Models.Enums.Zonas Zona { get; set; }

        [Display(Name = "Razón Social")]
        public string RazonSocial { get; set; }
    }
}