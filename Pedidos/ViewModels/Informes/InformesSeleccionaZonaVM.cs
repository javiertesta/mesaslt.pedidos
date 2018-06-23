using System.ComponentModel.DataAnnotations;

namespace Pedidos.ViewModels
{
    public class InformesSeleccionaZonaVM
    {

        [Required]
        [Display(Name = "Zona", ShortName = "Zona")]
        public Pedidos.Models.Enums.Zonas id { get; set; }

    }
}