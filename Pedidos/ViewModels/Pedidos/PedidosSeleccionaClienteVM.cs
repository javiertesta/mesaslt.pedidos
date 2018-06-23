using CustomExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Pedidos.ViewModels.ChildObjects;
using Pedidos.Models;

namespace Pedidos.ViewModels
{
    public class PedidosSeleccionaClienteVM
    {
        [CustomClienteId(CustomClienteIdAttribute.Options.shouldExist, false)]
        [Display(Name = "Código de Cliente", ShortName = "Código")]
        public string Id { get; set; }
    }
}