using Pedidos.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pedidos.ViewModels
{

    public class ElementoHTML
    {

        public string Nombre { get; set; }

        public string Placeholder { get; set; }

    }

    public class JumbotronHTML : ElementoHTML
    {

        public string Titulo { get; set; }

        public string Descripción { get; set; }

    }

    public class BotonHTML : ElementoHTML
    {

        public string Enlace { get; set; }

        public bool Disabled { get; set; }

        public string Etiqueta { get; set; }

    }

    public class FormularioHTML
    {

        public FormularioHTML()
        {
            this.BotonDeEnvio = new BotonHTML();
        }
        
        public string Accion { get; set; }

        public string Controlador { get; set; }

        public FormMethod Metodo { get; set; }

        public BotonHTML BotonDeEnvio { get; set; }

    }

}