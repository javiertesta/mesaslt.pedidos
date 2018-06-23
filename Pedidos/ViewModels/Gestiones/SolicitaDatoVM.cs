using CustomExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pedidos.ViewModels.Gestiones
{

    public class SolicitaDatoVM
    {

        public SolicitaDatoVM()
        {
            this.Jumbotron = new JumbotronHTML();
            this.BotonAlta = new BotonHTML();
            this.BotonPrimario = new BotonHTML();
            this.CuadroDeTexto = new ElementoHTML();
        }

        public string Titulo { get; set; }

        public JumbotronHTML Jumbotron { get; set; }

        [CustomClienteId(CustomClienteIdAttribute.Options.shouldExist, false)]
        [Display(Name = "Código de Cliente", ShortName = "Código")]
        public Object Id { get; set; }

        public string Controlador { get; set; }

        public string Accion { get; set; }

        public BotonHTML BotonPrimario { get; set; }

        public BotonHTML BotonAlta { get; set; }

        public ElementoHTML CuadroDeTexto { get; set; }

        public string Origen { get; set; }
        
    }

}