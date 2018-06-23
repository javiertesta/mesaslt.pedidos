using Pedidos.DAL;
using Pedidos.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;

namespace Pedidos.ViewModels
{

    public class PedidosIndexVM
    {

        public string Titulo = "";
        public string Action = "";
        public string Controller = "";
        public string VolverA = "";
        public RouteValueDictionary BajaRV;
        public string Parametro;
        public string Modo;
        public PagedList.IPagedList<Pedido> Pedidos;
        public SelectList SearchInList;

        public enum TiposDeArticulos
        {
            Tapa = 0,
            Base = 1,
            Vitrea = 2,
            FueraDeLista = 3
        }

        public Dictionary<Type, TiposDeArticulos> Tipos = new Dictionary<Type, TiposDeArticulos>
        {
            {typeof(Tapa), TiposDeArticulos.Tapa},
            {typeof(Base), TiposDeArticulos.Base},
            {typeof(Vitrea), TiposDeArticulos.Vitrea},
            {typeof(FueraDeLista), TiposDeArticulos.FueraDeLista}
        };

        public PedidosIndexVM()
        {
            var searchOptions = new List<SelectListItem>
            {
                    new SelectListItem{Text = "Buscar en Todo", Value = "all"},
                    new SelectListItem{Text = "General -> Código de Pedido", Value = "id"},
                    new SelectListItem{Text = "General -> Cantidad Pedida", Value = "cantidad"},
                    new SelectListItem{Text = "General -> Tipo de Artículo", Value = "tipodearticulo"},
                    new SelectListItem{Text = "General -> Código Tango Solicitado", Value = "estructurasolicitada"},
                    new SelectListItem{Text = "General -> Fecha de Entrega", Value = "fechaentrega"},
                    new SelectListItem{Text = "General -> Referencia del Cliente", Value = "referencia"},
                    new SelectListItem{Text = "General -> Particularidades del Artículo", Value = "particularidades"},
                    new SelectListItem{Text = "General -> Observaciones del Artículo", Value = "observaciones"},
                    new SelectListItem{Text = "General -> Fecha de Baja", Value = "fechabaja"},
                    new SelectListItem{Text = "Tapas -> Tipo", Value = "tapatipo"},
                    new SelectListItem{Text = "Tapas -> Medida", Value = "tapamedida"},
                    new SelectListItem{Text = "Tapas -> Código de Laminado", Value = "tapalaminadocodigo"},
                    new SelectListItem{Text = "Tapas -> Código de Muestrario de Laminado", Value = "tapalaminadomuestrario"},
                    new SelectListItem{Text = "Tapas -> Melamina", Value = "tapamelamina"},
                    new SelectListItem{Text = "Tapas -> Tipo de Borde", Value = "tapabordetipo"},
                    new SelectListItem{Text = "Tapas -> Color del Borde", Value = "tapabordecolor"},
                    new SelectListItem{Text = "Tapas -> Espesor del Borde", Value = "tapabordeespesor"},
                    new SelectListItem{Text = "Bases -> Modelo", Value = "basemodelo"},
                    new SelectListItem{Text = "Bases -> Espesor del Caño", Value = "baseespesor"},
                    new SelectListItem{Text = "Bases -> Color", Value = "basecolor"},
                    new SelectListItem{Text = "Bases -> Proveedor", Value = "baseproveedor"},
                    new SelectListItem{Text = "Vitreas -> Tipo", Value = "vitreatipo"},
                    new SelectListItem{Text = "Vitreas -> Medida", Value = "vitreamedida"},
                    new SelectListItem{Text = "Vitreas -> Transparente", Value = "vitreatransparente"},
                    new SelectListItem{Text = "Fuera de Medida -> Titulo", Value = "fuerademedidatitulo"},
                    new SelectListItem{Text = "Fuera de Medida -> Detalle", Value = "fuerademedidadetalle"}
            };
            SearchInList = new SelectList(searchOptions, "Value", "Text", searchOptions.ToArray()[0]);
        }
    }

    public class PedidosHistorialVM
    {

        public string Titulo = "";
        public string Action = "";
        public string Controller = "";

        public List<Pedido> _pedidos;
        private List<Tuple<DateTime, string, int>> _orden = new List<Tuple<DateTime, string, int>>();
        public Dictionary<int, List<CambioDeSeguimiento>> _seguimientos;

        public List<Pedido> Pedidos
        {
            get
            {
                return _pedidos;
            }

            set
            {
                _pedidos = value;
                OrdenaDatos();
            }

        }

        public Dictionary<int, List<CambioDeSeguimiento>> VariacionesDeSeguimientos
        {
            get
            {
                return _seguimientos;
            }

            set
            {
                _seguimientos = value;
                OrdenaDatos();
            }

        }

        public Dictionary<int, List<CambioDeSeguimiento>> SeguimientosCompletos { get; set; }

        public void OrdenaDatos()
        {

            _orden.Clear();

            // Dispone Pedidos
            if (_pedidos != null && _pedidos.Count !=0)
            {
                DateTime fechaPedidoAnterior = _pedidos.Last().Gestion.FechaGestion;
                foreach (var pedido in _pedidos)
                {
                    Tuple<DateTime, string, int> _item = new Tuple<DateTime, string, int>(fechaPedidoAnterior, "pedido", pedido.PedidoId);
                    _orden.Add(_item);
                    fechaPedidoAnterior = pedido.FechaBaja.HasValue ? pedido.FechaBaja.Value : DateTime.Now;
                }
            }

            // Dispone Seguimientos
            if (_seguimientos != null && _seguimientos.Count != 0)
            {
                int primerPedido = _seguimientos.FirstOrDefault().Key;
                for (int i = 0; i < _seguimientos[primerPedido].Count; i++)
                {
                    Tuple<DateTime, string, int> _item = new Tuple<DateTime, string, int>(_seguimientos[primerPedido][i].Fecha, "seguimiento", i);
                    _orden.Add(_item);
                }
            }

            // Ordena
            _orden.Sort((x, y) =>
            {
                return x.Item1.CompareTo(y.Item1);
            });

        }

        public List<Tuple<DateTime, string, int>> Orden
        {
            get { return _orden; }
        }

    }

}