using CustomExtensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pedidos.ViewModels
{
    public class GestionesDetalleVM
    {
    
        [Display(Name = "Código de Gestión")]
        public int GestionId { get; set; }

        [Display(Name = "Fecha")]
        public DateTime FechaGestion { get; set; }

        [Display(Name = "Cliente")]
        public string ClienteId { get; set; }

        [Display(Name = "Usuario")]
        public string UserName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Observaciones { get; set; }

        public string VolverA { get; set; }

    }

}