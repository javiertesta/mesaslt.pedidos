using CustomExtensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pedidos.ViewModels
{
    public class GestionesModificacionVM
    {
        public int GestionId { get; set; }

        [CustomDate]
        [Required]
        [Display(Name = "Fecha")]
        public System.DateTime FechaGestion { get; set; }

        [DataType(DataType.MultilineText)]
        public string Observaciones { get; set; }

        public byte[] RowVersion { get; set; }

        public string VolverA { get; set; }
    }
}