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
    public class ArticulosEditVM
    {
        public ArticulosEditVM()
        {
            this.items = new List<ArticulosEditChildVM>();
        }

        public ArticulosEditVM(IEnumerable<Articulo> articulos) : this()
        {
            ArticulosEditChildVM VMitem;
            foreach (var articulo in articulos)
            {
                VMitem = new ArticulosEditChildVM();

                VMitem.CodigoTango = articulo.CodigoTango;
                VMitem.ArticuloId = articulo.ArticuloId;
                VMitem.Particularidades = articulo.Particularidades;
                VMitem.RowVersion = articulo.RowVersion;
                VMitem.TipoDeArticulo = articulo.TipoDeArticulo;

                switch (articulo.TipoDeArticulo)
                {
                    case "Base":
                        VMitem.BAS_Color = (articulo as Base).Color;
                        VMitem.BAS_Espesor = (articulo as Base).Espesor;
                        VMitem.BAS_Modelo = (articulo as Base).Modelo;
                        VMitem.BAS_Proveedor = (articulo as Base).Proveedor;
                        break;

                    case "Tapa":
                        VMitem.TAP_BOR_Color = (articulo as Tapa).Borde.Color;
                        VMitem.TAP_BOR_Espesor = (articulo as Tapa).Borde.Espesor;
                        VMitem.TAP_BOR_Tipo = (articulo as Tapa).Borde.Tipo;
                        VMitem.TAP_LAM_Codigo = (articulo as Tapa).Laminado_CodigoId;
                        VMitem.TAP_LAM_MuestrarioId = (articulo as Tapa).Laminado_MuestrarioId;
                        VMitem.TAP_Medida = (articulo as Tapa).Medida;
                        VMitem.TAP_Melamina = (articulo as Tapa).Melamina;
                        VMitem.TAP_Tipo = (articulo as Tapa).Tipo;
                        break;
                    
                    case "Vitrea":
                        VMitem.VIT_Medida = (articulo as Vitrea).Medida;
                        VMitem.VIT_Tipo = (articulo as Vitrea).Tipo;
                        VMitem.VIT_Transparente = (articulo as Vitrea).Transparente;
                        break;

                    default:
                        break;
                }
                this.items.Add(VMitem);
            }
        }

        public string ReturnURL { get; set; }

        public List<ArticulosEditChildVM> items { get; set; }

    }
}

namespace Pedidos.ViewModels.ChildObjects
{
    public class ArticulosEditChildVM : ArticulosCreateChildVM
    {
        public ArticulosEditChildVM() { }

        public override bool Difiere(Articulo articulo)
        {
            if (articulo == null) return true;
            if (base.Difiere(articulo)) return true;

            if ((this.ArticuloId != articulo.ArticuloId) ||
                (this.RowVersion != articulo.RowVersion)) return true;
            
            return false;
        }

        public override void CopiarEn(Articulo articulo)
        {
            // Si no coinciden los tipos, regresa.
            if (this.TipoDeArticulo != articulo.TipoDeArticulo) return;

            base.CopiarEn(articulo);

            // Copia los datos del artículo.
            articulo.RowVersion = this.RowVersion;
            articulo.ArticuloId = this.ArticuloId;
        }

        public override void CopiarDesde(Articulo articulo)
        {
            base.CopiarDesde(articulo);

            this.ArticuloId = articulo.ArticuloId;
            this.RowVersion = articulo.RowVersion;
        }

        [Display(Name = "Código de Artículo", ShortName = "Artículo")]
        public int ArticuloId { get; set; }
        
        public byte[] RowVersion { get; set; }
    }
}