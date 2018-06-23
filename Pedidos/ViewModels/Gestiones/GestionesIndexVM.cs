using CustomExtensions;
using Pedidos.Models;
using Pedidos.ViewModels.ChildObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Pedidos.ViewModels
{

    public class GestionesIndexVM
    {

        public string Titulo = "Listado de Gestiones";
        public string Controller = "Gestiones";
        public string ActionMethod = "Index";
        public PagedList.IPagedList<Gestion> Gestiones;
        public SelectList SearchInList;
        public Dictionary<string, string> SortLinks = new Dictionary<string, string>();

        public string ClienteId { get; set; }
        
        public GestionesIndexVM()
        {
            SelectListItem selected = new SelectListItem { Text = "Todos los campos", Value = "all" };
            SearchInList = new SelectList(new List<SelectListItem>
                {
                    selected,
                    new SelectListItem{Text = "Código de Gestion", Value = "codigo"},
                    new SelectListItem{Text = "Código de Cliente", Value = "cliente"},
                    new SelectListItem{Text = "Fecha de Gestion", Value = "fecha"},
                    new SelectListItem{Text = "Usuario que Intervino", Value = "usuario"},
                    new SelectListItem{Text = "Observaciones", Value = "observaciones"}
                }, "Value", "Text", selected);

            SortLinks.Add("gestion", "gestionid");
            SortLinks.Add("fechagestion", "fechagestion");
            SortLinks.Add("username", "username");
            SortLinks.Add("observaciones", "observaciones");
            SortLinks.Add("clienteid", "clienteid");
        }

    }

}