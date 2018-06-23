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
    public class ArticulosCreateVM
    {
        public string ReturnURL { get; set; }

        public List<ArticulosCreateChildVM> items { get; set; }

        public ArticulosCreateVM()
        {
            this.items = new List<ArticulosCreateChildVM>();
        }

        public ArticulosCreateVM(IEnumerable<Articulo> articulos) : this()
        {
            ArticulosCreateChildVM VMitem;
            foreach (var articulo in articulos)
            {
                VMitem = new ArticulosCreateChildVM();
                VMitem.CopiarDesde(articulo);
                this.items.Add(VMitem);
            }
        }

    }
}

namespace Pedidos.ViewModels.ChildObjects
{
    public class ArticulosCreateChildVM
    {
        public virtual bool Difiere(Articulo articulo)
        {
            if (articulo == null) return true;
            bool _output = false;

            if (this.TipoDeArticulo != articulo.TipoDeArticulo) _output = true;
            if (this.Particularidades != articulo.Particularidades) _output = true;

            switch (this.TipoDeArticulo)
            {

                case "Base":
                    if ((this.BAS_Color != (articulo as Base).Color) ||
                        (this.BAS_Espesor != (articulo as Base).Espesor) ||
                        (this.BAS_Modelo != (articulo as Base).Modelo) ||
                        (this.BAS_Proveedor != (articulo as Base).Proveedor)) _output = true;
                    break;

                case "Tapa":
                    if ((this.TAP_BOR_Color != (articulo as Tapa).Borde.Color) ||
                        (this.TAP_BOR_Espesor != (articulo as Tapa).Borde.Espesor) ||
                        (this.TAP_BOR_Tipo != (articulo as Tapa).Borde.Tipo) ||
                        (this.TAP_LAM_Codigo != (articulo as Tapa).Laminado_CodigoId) ||
                        (this.TAP_LAM_MuestrarioId != (articulo as Tapa).Laminado_MuestrarioId) ||
                        (this.TAP_Medida != (articulo as Tapa).Medida) ||
                        (this.TAP_Melamina != (articulo as Tapa).Melamina) ||
                        (this.TAP_Tipo != (articulo as Tapa).Tipo)) _output = true;
                    break;

                case "Vitrea":
                    if ((this.VIT_Medida != (articulo as Vitrea).Medida) ||
                        (this.VIT_Tipo != (articulo as Vitrea).Tipo) ||
                        (this.VIT_Transparente != (articulo as Vitrea).Transparente)) _output = true;
                    break;

                case "FueraDeLista":
                    if ((this.FUE_Titulo != (articulo as FueraDeLista).Titulo) ||
                        (this.FUE_Detalle != (articulo as FueraDeLista).Detalle)) _output = true;
                    break;

                default:
                    _output = true;
                    break;
            }
            return _output;
        }

        public virtual void CopiarEn(Articulo articulo)
        {
            // Si no coinciden los tipos, regresa.
            if (this.TipoDeArticulo != articulo.TipoDeArticulo) return;

            // Copia los datos del artículo.
            articulo.Particularidades = this.Particularidades;
            articulo.CodigoTango = this.CodigoTango;

            // Copia los datos específicos de cada artículo.
            switch (articulo.TipoDeArticulo)
            {
                case "Tapa":
                    ((Tapa)articulo).Borde.Color = this.TAP_BOR_Color;
                    ((Tapa)articulo).Borde.Espesor = this.TAP_BOR_Espesor;
                    ((Tapa)articulo).Borde.Tipo = this.TAP_BOR_Tipo;
                    ((Tapa)articulo).Laminado_CodigoId = this.TAP_LAM_Codigo;
                    ((Tapa)articulo).Laminado_MuestrarioId = this.TAP_LAM_MuestrarioId;
                    ((Tapa)articulo).Medida = this.TAP_Medida;
                    ((Tapa)articulo).Melamina = this.TAP_Melamina;
                    ((Tapa)articulo).Tipo = this.TAP_Tipo;
                    break;
                case "Base":
                    ((Base)articulo).Color = this.BAS_Color;
                    ((Base)articulo).Espesor = this.BAS_Espesor;
                    ((Base)articulo).Modelo = this.BAS_Modelo;
                    ((Base)articulo).Proveedor = this.BAS_Proveedor;
                    break;
                case "Vitrea":
                    ((Vitrea)articulo).Medida = this.VIT_Medida;
                    ((Vitrea)articulo).Tipo = this.VIT_Tipo;
                    ((Vitrea)articulo).Transparente = this.VIT_Transparente;
                    break;
                case "FueraDeLista":
                    ((FueraDeLista)articulo).Titulo = this.FUE_Titulo;
                    ((FueraDeLista)articulo).Detalle = this.FUE_Detalle;
                    break;
            }
        }

        public virtual void CopiarDesde(Articulo articulo)
        {
            this.Particularidades = articulo.Particularidades;
            this.TipoDeArticulo = articulo.TipoDeArticulo;
            this.CodigoTango = articulo.CodigoTango;

            switch (articulo.TipoDeArticulo)
            {
                case "Base":
                    this.BAS_Color = ((articulo) as Base).Color;
                    this.BAS_Espesor = ((articulo) as Base).Espesor;
                    this.BAS_Modelo = ((articulo) as Base).Modelo;
                    this.BAS_Proveedor = ((articulo) as Base).Proveedor;
                    break;

                case "Tapa":
                    this.TAP_BOR_Color = ((articulo) as Tapa).Borde.Color;
                    this.TAP_BOR_Espesor = ((articulo) as Tapa).Borde.Espesor;
                    this.TAP_BOR_Tipo = ((articulo) as Tapa).Borde.Tipo;
                    this.TAP_LAM_Codigo = ((articulo) as Tapa).Laminado_CodigoId;
                    this.TAP_LAM_MuestrarioId = ((articulo) as Tapa).Laminado_MuestrarioId;
                    this.TAP_Medida = ((articulo) as Tapa).Medida;
                    this.TAP_Melamina = ((articulo) as Tapa).Melamina;
                    this.TAP_Tipo = ((articulo) as Tapa).Tipo;
                    break;

                case "Vitrea":
                    this.VIT_Medida = ((articulo) as Vitrea).Medida;
                    this.VIT_Tipo = ((articulo) as Vitrea).Tipo;
                    this.VIT_Transparente = ((articulo) as Vitrea).Transparente;
                    break;

                case "FueraDeLista":
                    this.FUE_Titulo = ((articulo) as FueraDeLista).Titulo;
                    this.FUE_Detalle = ((articulo) as FueraDeLista).Detalle;
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

        [Required]
        [Display(Name = "Código Tango")]
        public string CodigoTango { get; set; }

        [Display(Name = "Particularidades")]
        public string Particularidades { get; set; }

        [Display(Name = "Tipo de Artículo", ShortName = "Artículo")]
        public string TipoDeArticulo { get; set; }

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