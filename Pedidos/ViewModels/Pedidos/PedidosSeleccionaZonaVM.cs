using System.ComponentModel.DataAnnotations;

namespace Pedidos.ViewModels
{
    public class PedidosSeleccionaZonaVM
    {
        [Required]
        [Display(Name = "Zona", ShortName = "Zona")]
        public Pedidos.Models.Enums.Zonas zonaId { get; set; }
    }
}