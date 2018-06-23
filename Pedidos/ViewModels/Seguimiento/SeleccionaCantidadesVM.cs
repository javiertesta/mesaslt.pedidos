using Pedidos.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pedidos.Models;

namespace Pedidos.ViewModels.Seguimiento
{
    
    public class SeleccionaCantidadesVM
    {
        public SeleccionaCantidadesVM()
        {
            this.PedidosEtapa1 = new List<SeleccionaCantidadesVM_Pedido>();
            var EtapasInternas = new List<SelectListItem>();
            this.RegresarA = RegresarA;
            using (PedidosDbContext db = new PedidosDbContext())
            {
                db.EtapasDelNegocioInternas.OrderBy(e => e.Nivel).ToList().ForEach(e =>
                {
                    var item = new SelectListItem { Text = e.Descripcion, Value = e.EtapaDelNegocioInternaId.ToString() };
                    EtapasInternas.Add(item);
                });
            }
            this.ListaDeEtapasInternas = new SelectList(EtapasInternas, "Value", "Text", EtapasInternas[0]);
        }

        public SeleccionaCantidadesVM(string RegresarA) : this()
        {
            this.RegresarA = RegresarA;
        }

        public int? NuevaEtapa { get; set; }

        public string NuevaEtapa_Nombre
        {
            get
            {
                using (PedidosDbContext db = new PedidosDbContext())
                {
                    return (this.NuevaEtapa == null ? "" : db.EtapasDelNegocioInternas.Find(this.NuevaEtapa).Descripcion);
                }
            }
        }

        public string RegresarA { get; set; }

        public SelectList ListaDeEtapasInternas { get; set; }

        public List<SeleccionaCantidadesVM_Pedido> PedidosEtapa1 { get; set; }
    }

    public class SeleccionaCantidadesVM_Pedido
    {

        public SeleccionaCantidadesVM_Pedido() { }

        public string ClienteId { get; set; }

        public int PedidoId { get; set; }

        public string Leyenda { get; set; }

        public int CantidadSeleccionada { get; set; }

    }

}