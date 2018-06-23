using System.ComponentModel.DataAnnotations;

namespace Pedidos.ViewModels
{
    public class PedidosSeleccionaGestionVM
    {
        [Display(Name = "Código de Gestión", ShortName = "Código")]
        public int gestionId { get; set; }
    }
}