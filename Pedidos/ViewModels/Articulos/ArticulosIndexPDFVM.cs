using CustomExtensions;
using Pedidos.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web;
using System.Web.Routing;

namespace Pedidos.ViewModels
{
    public class ArticulosIndexPDFVM
    {

        public string Titulo = "";
        public string Action = "";
        public string Controller = "";
        public string VolverA = "";
        public RouteValueDictionary BajaRV;
        public string Parametro;
        public string Modo;
        public List<Articulo> Items;
        public SelectList SearchInList;

        public enum TiposDeArticulos
        {
            Tapa = 0,
            Base = 1,
            Vitrea = 2
        }

        public Dictionary<Type, TiposDeArticulos> Tipos = new Dictionary<Type, TiposDeArticulos>
        {
            {typeof(Tapa), TiposDeArticulos.Tapa},
            {typeof(Base), TiposDeArticulos.Base},
            {typeof(Vitrea), TiposDeArticulos.Vitrea}
        };

        public ArticulosIndexPDFVM()
        {
            var searchOptions = new List<SelectListItem>
            {
                    new SelectListItem{Text = "Buscar en Todo", Value = "all"},
                    new SelectListItem{Text = "General -> Tipo de Artículo", Value = "tipodearticulo"},
                    new SelectListItem{Text = "General -> Código Tango", Value = "codigotango"},
                    new SelectListItem{Text = "General -> Particularidades del Artículo", Value = "particularidades"},
                    new SelectListItem{Text = "Tapas -> Tipo", Value = "tapatipo"},
                    new SelectListItem{Text = "Tapas -> Medida", Value = "tapamedida"},
                    new SelectListItem{Text = "Tapas -> Melamina", Value = "tapamelamina"},
                    new SelectListItem{Text = "Tapas -> Tipo de Borde", Value = "tapabordetipo"},
                    new SelectListItem{Text = "Tapas -> Espesor del Borde", Value = "tapabordeespesor"},
                    new SelectListItem{Text = "Bases -> Modelo", Value = "basemodelo"},
                    new SelectListItem{Text = "Bases -> Espesor del Caño", Value = "baseespesor"},
                    new SelectListItem{Text = "Bases -> Proveedor", Value = "baseproveedor"},
                    new SelectListItem{Text = "Vitreas -> Tipo", Value = "vitreatipo"},
                    new SelectListItem{Text = "Vitreas -> Medida", Value = "vitreamedida"},
                    new SelectListItem{Text = "Vitreas -> Transparente", Value = "vitreatransparente"}
            };
            SearchInList = new SelectList(searchOptions, "Value", "Text", searchOptions.ToArray()[0]);
        }
    }
}