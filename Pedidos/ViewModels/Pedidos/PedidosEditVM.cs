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
    public class PedidosEditVM
    {
        public PedidosEditVM()
        {
            this.items = new List<PedidosEditChildVM>();
        }

        public PedidosEditVM(IEnumerable<Pedido> pedidos) : this()
        {
            PedidosEditChildVM VMitem;
            foreach (var pedido in pedidos)
            {
                VMitem = new PedidosEditChildVM();

                VMitem.Cantidad = pedido.Cantidad;
                VMitem.FechaBaja = pedido.FechaBaja;
                VMitem.FechaEntrega = pedido.FechaEntrega;
                VMitem.PedidoId = pedido.PedidoId;
                VMitem.Referencia = pedido.Referencia;
                VMitem.RowVersion = pedido.RowVersion;
                VMitem.EstructuraSolicitada = pedido.EstructuraSolicitada;

                VMitem.ART_Particularidades = pedido.Articulo.Particularidades;
                VMitem.ART_RowVersion = pedido.Articulo.RowVersion;
                VMitem.TipoDeArticulo = pedido.Articulo.TipoDeArticulo;

                switch (pedido.Articulo.TipoDeArticulo)
                {
                    case "Base":
                        VMitem.BAS_Color = ((pedido.Articulo) as Base).Color;
                        VMitem.BAS_Espesor = ((pedido.Articulo) as Base).Espesor;
                        VMitem.BAS_Modelo = ((pedido.Articulo) as Base).Modelo;
                        VMitem.BAS_Proveedor = ((pedido.Articulo) as Base).Proveedor;
                        break;

                    case "Tapa":
                        VMitem.TAP_BOR_Color = ((pedido.Articulo) as Tapa).Borde.Color;
                        VMitem.TAP_BOR_Espesor = ((pedido.Articulo) as Tapa).Borde.Espesor;
                        VMitem.TAP_BOR_Tipo = ((pedido.Articulo) as Tapa).Borde.Tipo;
                        VMitem.TAP_LAM_Codigo = ((pedido.Articulo) as Tapa).Laminado_CodigoId;
                        VMitem.TAP_LAM_MuestrarioId = ((pedido.Articulo) as Tapa).Laminado_MuestrarioId;
                        VMitem.TAP_Medida = ((pedido.Articulo) as Tapa).Medida;
                        VMitem.TAP_Melamina = ((pedido.Articulo) as Tapa).Melamina;
                        VMitem.TAP_Tipo = ((pedido.Articulo) as Tapa).Tipo;
                        break;
                    
                    case "Vitrea":
                        VMitem.VIT_Medida = ((pedido.Articulo) as Vitrea).Medida;
                        VMitem.VIT_Tipo = ((pedido.Articulo) as Vitrea).Tipo;
                        VMitem.VIT_Transparente = ((pedido.Articulo) as Vitrea).Transparente;
                        break;

                    case "FueraDeLista":
                        VMitem.FUE_Titulo = ((pedido.Articulo) as FueraDeLista).Titulo;
                        VMitem.FUE_Detalle = ((pedido.Articulo) as FueraDeLista).Detalle;
                        break;

                    default:
                        break;
                }
                this.items.Add(VMitem);
            }
        }

        public string VolverA { get; set; }

        public List<PedidosEditChildVM> items { get; set; }

    }
}

namespace Pedidos.ViewModels.ChildObjects
{
    public class PedidosEditChildVM : PedidosCreateStep2ChildVM
    {
        public PedidosEditChildVM() { }

        public override bool Difiere(Pedido pedido)
        {
            if (pedido == null) return true;
            if (base.Difiere(pedido)) return true;

            if (this.PedidoId != pedido.PedidoId) return true;
            
            return false;
        }

        public override void CopiarEn(Pedido pedido)
        {
            // Si no coinciden los tipos, regresa.
            if (this.TipoDeArticulo != pedido.Articulo.TipoDeArticulo) return;
            
            // La cantidad pedida no debe ser modificada.
            var Cantidad = pedido.Cantidad;
            base.CopiarEn(pedido);
            pedido.Cantidad = Cantidad;

            // Copia los datos del pedido.
            pedido.RowVersion = this.RowVersion;
            pedido.PedidoId = this.PedidoId;

            // Copia los datos del artículo.
            pedido.Articulo.RowVersion = this.ART_RowVersion;
            pedido.Articulo.ArticuloId = this.PedidoId;
        }

        public override void CopiarDesde(Pedido pedido)
        {
            base.CopiarDesde(pedido);

            this.PedidoId = pedido.PedidoId;
            this.RowVersion = pedido.RowVersion;
            this.ART_RowVersion = pedido.RowVersion;
        }

        [Display(Name = "Código de Pedido", ShortName = "Pedido")]
        public int PedidoId { get; set; }

        public byte[] ART_RowVersion { get; set; }

        public byte[] RowVersion { get; set; }
    }
}