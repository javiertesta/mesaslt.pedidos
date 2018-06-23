using CustomExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pedidos.ViewModels
{
    public class ClientesDeleteVM
    {
        [Display(Name = "Código")]
        public string ClienteId { get; set; }

        [Display(Name = "Zona Geográfica")]
        public string ZonaNombre { get; set; }

        [Display(Name = "Razón Social")]
        public string RazonSocial { get; set; }
    }
}