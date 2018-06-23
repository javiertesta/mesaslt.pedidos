using CustomExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Pedidos.ViewModels.ChildObjects;
using Pedidos.Models;
using Pedidos.DAL;
using System.Data.Entity;

namespace Pedidos.ViewModels
{
    public class PedidosCreateStep2VM
    {
        public int GestionId { get; set; }

        public string VolverA { get; set; }

        public bool ModoRapido { get; set; }

        public List<PedidosCreateStep2ChildVM> items { get; set; }

        public PedidosCreateStep2VM()
        {
            this.items = new List<PedidosCreateStep2ChildVM>();
        }

        public PedidosCreateStep2VM(int GestionId) : this()
        {
            this.GestionId = GestionId;
        }

        public PedidosCreateStep2VM(IEnumerable<Pedido> pedidos, int GestionId) : this(GestionId)
        {
            PedidosCreateStep2ChildVM VMitem;
            foreach (var pedido in pedidos)
            {
                VMitem = new PedidosCreateStep2ChildVM();
                VMitem.CopiarDesde(pedido);
                this.items.Add(VMitem);
            }
        }

    }
}

namespace Pedidos.ViewModels.ChildObjects
{
    public class PedidosCreateStep2ChildVM
    {
        public virtual bool Difiere(Pedido pedido)
        {
            if (pedido == null) return true;
            bool _output = false;

            if (this.TipoDeArticulo != pedido.Articulo.TipoDeArticulo) _output = true;

            if ((this.Cantidad != pedido.Cantidad) ||
                (this.FechaBaja != pedido.FechaBaja) ||
                (this.FechaEntrega != pedido.FechaEntrega) ||
                (this.Referencia != pedido.Referencia) ||
                (this.EstructuraSolicitada != pedido.EstructuraSolicitada) ||
                (this.Observaciones != pedido.Observaciones)) _output = true;

            if (this.ART_Particularidades != pedido.Articulo.Particularidades) _output = true;

            switch (this.TipoDeArticulo)
            {

                case "Base":
                    if ((this.BAS_Color != (pedido.Articulo as Base).Color) ||
                        (this.BAS_Espesor != (pedido.Articulo as Base).Espesor) ||
                        (this.BAS_Modelo != (pedido.Articulo as Base).Modelo) ||
                        (this.BAS_Proveedor != (pedido.Articulo as Base).Proveedor)) _output = true;
                    break;

                case "Tapa":
                    if ((this.TAP_BOR_Color != (pedido.Articulo as Tapa).Borde.Color) ||
                        (this.TAP_BOR_Espesor != (pedido.Articulo as Tapa).Borde.Espesor) ||
                        (this.TAP_BOR_Tipo != (pedido.Articulo as Tapa).Borde.Tipo) ||
                        (this.TAP_LAM_Codigo != (pedido.Articulo as Tapa).Laminado_CodigoId) ||
                        (this.TAP_LAM_MuestrarioId != (pedido.Articulo as Tapa).Laminado_MuestrarioId) ||
                        (this.TAP_Medida != (pedido.Articulo as Tapa).Medida) ||
                        (this.TAP_Melamina != (pedido.Articulo as Tapa).Melamina) ||
                        (this.TAP_Tipo != (pedido.Articulo as Tapa).Tipo)) _output = true;
                    break;

                case "Vitrea":
                    if ((this.VIT_Medida != (pedido.Articulo as Vitrea).Medida) ||
                        (this.VIT_Tipo != (pedido.Articulo as Vitrea).Tipo) ||
                        (this.VIT_Transparente != (pedido.Articulo as Vitrea).Transparente)) _output = true;
                    break;

                case "FueraDeLista":
                    if ((this.FUE_Titulo != (pedido.Articulo as FueraDeLista).Titulo) ||
                        (this.FUE_Detalle != (pedido.Articulo as FueraDeLista).Detalle)) _output = true;
                    break;

                default:
                    _output = true;
                    break;

            }

            return _output;

        }

        public virtual void CopiarEn(Pedido pedido)
        {
            // Si no coinciden los tipos, regresa.
            if (this.TipoDeArticulo != pedido.Articulo.TipoDeArticulo) return;

            // Copia los datos del pedido.
            pedido.Cantidad = this.Cantidad;
            pedido.FechaEntrega = this.FechaEntrega;
            pedido.Referencia = this.Referencia;
            pedido.EstructuraSolicitada = this.EstructuraSolicitada;
            pedido.Observaciones = this.Observaciones;

            // Copia los datos del artículo.
            pedido.Articulo.Particularidades = this.ART_Particularidades;

            // Copia los datos específicos de cada artículo.
            switch (pedido.Articulo.TipoDeArticulo)
            {
                case "Tapa":
                    ((Tapa)pedido.Articulo).Borde.Color = this.TAP_BOR_Color;
                    ((Tapa)pedido.Articulo).Borde.Espesor = this.TAP_BOR_Espesor;
                    ((Tapa)pedido.Articulo).Borde.Tipo = this.TAP_BOR_Tipo;
                    ((Tapa)pedido.Articulo).Laminado_CodigoId = this.TAP_LAM_Codigo;
                    ((Tapa)pedido.Articulo).Laminado_MuestrarioId = this.TAP_LAM_MuestrarioId;
                    ((Tapa)pedido.Articulo).Medida = this.TAP_Medida;
                    ((Tapa)pedido.Articulo).Melamina = this.TAP_Melamina;
                    ((Tapa)pedido.Articulo).Tipo = this.TAP_Tipo;
                    break;
                case "Base":
                    ((Base)pedido.Articulo).Color = this.BAS_Color;
                    ((Base)pedido.Articulo).Espesor = this.BAS_Espesor;
                    ((Base)pedido.Articulo).Modelo = this.BAS_Modelo;
                    ((Base)pedido.Articulo).Proveedor = this.BAS_Proveedor;
                    break;
                case "Vitrea":
                    ((Vitrea)pedido.Articulo).Medida = this.VIT_Medida;
                    ((Vitrea)pedido.Articulo).Tipo = this.VIT_Tipo;
                    ((Vitrea)pedido.Articulo).Transparente = this.VIT_Transparente;
                    break;
                case "FueraDeLista":
                    ((FueraDeLista)pedido.Articulo).Titulo = this.FUE_Titulo;
                    ((FueraDeLista)pedido.Articulo).Detalle = this.FUE_Detalle;
                    break;
            }
        }

        public virtual void CopiarDesde(Pedido pedido)
        {
            this.Cantidad = pedido.Cantidad;
            this.FechaBaja = pedido.FechaBaja;
            this.FechaEntrega = pedido.FechaEntrega;
            this.Referencia = pedido.Referencia;
            this.EstructuraSolicitada = pedido.EstructuraSolicitada;
            this.Observaciones = pedido.Observaciones;

            this.ART_Particularidades = pedido.Articulo.Particularidades;
            this.TipoDeArticulo = pedido.Articulo.TipoDeArticulo;

            switch (pedido.Articulo.TipoDeArticulo)
            {
                case "Base":
                    this.BAS_Color = ((pedido.Articulo) as Base).Color;
                    this.BAS_Espesor = ((pedido.Articulo) as Base).Espesor;
                    this.BAS_Modelo = ((pedido.Articulo) as Base).Modelo;
                    this.BAS_Proveedor = ((pedido.Articulo) as Base).Proveedor;
                    break;

                case "Tapa":
                    this.TAP_BOR_Color = ((pedido.Articulo) as Tapa).Borde.Color;
                    this.TAP_BOR_Espesor = ((pedido.Articulo) as Tapa).Borde.Espesor;
                    this.TAP_BOR_Tipo = ((pedido.Articulo) as Tapa).Borde.Tipo;
                    this.TAP_LAM_Codigo = ((pedido.Articulo) as Tapa).Laminado_CodigoId;
                    this.TAP_LAM_MuestrarioId = ((pedido.Articulo) as Tapa).Laminado_MuestrarioId;
                    this.TAP_Medida = ((pedido.Articulo) as Tapa).Medida;
                    this.TAP_Melamina = ((pedido.Articulo) as Tapa).Melamina;
                    this.TAP_Tipo = ((pedido.Articulo) as Tapa).Tipo;
                    break;

                case "Vitrea":
                    this.VIT_Medida = ((pedido.Articulo) as Vitrea).Medida;
                    this.VIT_Tipo = ((pedido.Articulo) as Vitrea).Tipo;
                    this.VIT_Transparente = ((pedido.Articulo) as Vitrea).Transparente;
                    break;

                case "FueraDeLista":
                    this.FUE_Titulo = ((pedido.Articulo) as FueraDeLista).Titulo;
                    this.FUE_Detalle = ((pedido.Articulo) as FueraDeLista).Detalle;
                    break;

                default:
                    break;
            }
        }

        public Articulo NewInstance()
        {
            switch (this.TipoDeArticulo)
            {
                case "Tapa":
                    return new Tapa();
                case "Base":
                    return new Base();
                case "Vitrea":
                    return new Vitrea();
                case "FueraDeLista":
                    return new FueraDeLista();
                default:
                    return new Articulo();
            }
        }

        public int Cantidad { get; set; }

        [Display(Name = "Fecha de Entrega", ShortName = "Entrega")]
        [CustomDate]
        public DateTime? FechaEntrega { get; set; }

        [Display(Name = "Código Tango")]
        public string CodigoTango { get; set; }

        [Display(Name = "Particularidades")]
        public string ART_Particularidades { get; set; }

        [Display(Name = "Referencia del Cliente", ShortName = "Referencia")]
        public string Referencia { get; set; }

        [Display(Name = "Estructura Solicitada")]
        public string EstructuraSolicitada { get; set; }

        [Display(Name = "Observaciones del Pedido", ShortName = "Observaciones")]
        public string Observaciones { get; set; }

        [Display(Name = "Tipo de Artículo", ShortName = "Artículo")]
        public string TipoDeArticulo { get; set; }

        [Display(Name = "Fecha de Baja", ShortName = "Baja")]
        [CustomDate]
        public DateTime? FechaBaja { get; set; }

        [Display(Name = "Tipo de Tapa", ShortName = "Tipo")]
        public Pedidos.Models.Enums.TiposDeTapas? TAP_Tipo { get; set; }

        [Display(Name = "Medida")]
        public string TAP_Medida { get; set; }

        [Display(Name = "Melamina")]
        public bool? TAP_Melamina { get; set; }

        [Display(Name = "Muestrario")]
        public int? TAP_LAM_MuestrarioId { get; set; }

        [Display(Name = "Laminado")]
        public string TAP_LAM_Codigo { get; set; }

        [Display(Name = "Tipo de Borde", ShortName = "Borde")]
        public Pedidos.Models.Enums.TiposDeBordesDeTapas TAP_BOR_Tipo { get; set; }

        [Display(Name = "Espesor del Borde", ShortName = "Espesor")]
        public Pedidos.Models.Enums.EspesoresDeBordesDeTapas TAP_BOR_Espesor { get; set; }

        [Display(Name = "Color del Borde")]
        public Pedidos.Models.Enums.ColoresDeBordesDeTapas? TAP_BOR_Color { get; set; }

        [Display(Name = "Modelo de Base", ShortName = "Modelo")]
        public string BAS_Modelo { get; set; }

        [Display(Name = "Espesor")]
        public Pedidos.Models.Enums.EspesoresDeBases BAS_Espesor { get; set; }

        [Display(Name = "Color")]
        public Pedidos.Models.Enums.ColoresDeBases? BAS_Color { get; set; }

        [Display(Name = "Proveedor")]
        public Pedidos.Models.Enums.Proveedores BAS_Proveedor { get; set; }

        [Display(Name = "Tipo de Vitrea", ShortName = "Tipo")]
        public string VIT_Tipo { get; set; }

        [Display(Name = "Medida")]
        public string VIT_Medida { get; set; }

        [Display(Name = "Transparente")]
        public bool? VIT_Transparente { get; set; }

        [Display(Name = "Titulo")]
        public string FUE_Titulo { get; set; }

        [Display(Name = "Detalle"), DataType(DataType.MultilineText)]
        public string FUE_Detalle { get; set; }

    }
}