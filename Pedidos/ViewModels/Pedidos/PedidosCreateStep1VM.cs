using CustomExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pedidos.ViewModels
{
    public class PedidosCreateStep1VM
    {
        public PedidosCreateStep1VM() { }

        public PedidosCreateStep1VM(int GestionId)
        {
            this.GestionId = GestionId;
            this.Iteraciones = 1;
        }

        [Display(Name = "Código Impar")]
        public string Codigo1 { get; set; }

        [Display(Name = "Código Par")]
        public string Codigo2 { get; set; }

        public int Iteraciones { get; set; }

        [Display(Name ="Carga Rápida")]
        public bool ModoRapido { get; set; }

        [Display(Name = "Código de Gestión", ShortName = "Gestión")]
        public int GestionId { get; set; }

        [Display(Name = "Volver A")]
        public string VolverA { get; set; }
    }
}