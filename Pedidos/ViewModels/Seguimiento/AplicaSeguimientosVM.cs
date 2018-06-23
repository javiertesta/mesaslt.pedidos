using Pedidos.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pedidos.Models;

namespace Pedidos.ViewModels.Seguimiento
{
    
    public class AplicaSeguimientosVM
    {
        public AplicaSeguimientosVM()
        {
            this.PedidosEtapa2 = new List<AplicaSeguimientosVM_Pedido>();
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

        public AplicaSeguimientosVM(string RegresarA) : this()
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

        public List<AplicaSeguimientosVM_Pedido> PedidosEtapa2 { get; set; }
    }

    public class AplicaSeguimientosVM_Pedido
    {

        public AplicaSeguimientosVM_Pedido()
        {
            this.Seguimientos = new List<AplicaSeguimientosVM_Seguimiento>();
        }

        public string ClienteId { get; set; }

        public int PedidoId { get; set; }

        public string Leyenda { get; set; }

        public int Cantidad { get; set; }

        public List<AplicaSeguimientosVM_Seguimiento> Seguimientos { get; set; }

    }

    public class AplicaSeguimientosVM_Seguimiento
    {

        public AplicaSeguimientosVM_Seguimiento() { }

        public int SeguimientoIndividualId { get; set; }

        public int CantidadAfectada { get; set; }

        public int CantidadActual { get; set; }

        public int CantidadResultante
        {
            get
            {
                return (this.CantidadActual - this.CantidadAfectada);
            }
        }

        public int EtapaDelNegocioInternaId { get; set; }

        public string EtapaDelNegocioInterna_Nombre
        {
            get
            {
                using (PedidosDbContext db = new PedidosDbContext())
                {
                    return db.EtapasDelNegocioInternas.Find(this.EtapaDelNegocioInternaId).Descripcion;
                }
            }
        }

    }

}