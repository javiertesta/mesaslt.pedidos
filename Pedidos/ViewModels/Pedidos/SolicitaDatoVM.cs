using Pedidos.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pedidos.ViewModels.PedidosVM
{
    
    public class SolicitaDatoVM
    {

        public SolicitaDatoVM()
        {
            this.Formulario = new FormularioHTML();
            this.ControlPrincipal = new ElementoHTML();
            this.Jumbotron = new JumbotronHTML();
        }

        public string Titulo { get; set; }

        public JumbotronHTML Jumbotron { get; set; }

        [Required]
        public Zonas? zonaId { get; set; }

        [Required]
        public string clienteId { get; set; }

        [Required]
        public int? gestionId { get; set; }

        [Required]
        public int? id { get; set; }

        public FormularioHTML Formulario { get; set; }

        public ElementoHTML ControlPrincipal { get; set; }

        public string Origen { get; set; }

    }

}