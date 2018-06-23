using CustomExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Pedidos.ViewModels
{
    public class ClientesEditVM
    {
        [MaxLength(15)]
        [Display(Name = "Código")]
        public string ClienteId { get; set; }

        [Display(Name = "Zona Geográfica")]
        public Pedidos.Models.Enums.Zonas Zona { get; set; }

        [Display(Name = "Razón Social")]
        public string RazonSocial { get; set; }

        public byte[] RowVersion { get; set; }
    }
}