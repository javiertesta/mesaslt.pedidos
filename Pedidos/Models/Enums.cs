using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pedidos.Models.Enums
{
    public enum Proveedores
    {
        [Display(Name="Centro S.A.")]
        Centro = 0,
        [Display(Name = "Formicolor S.A.")]
        Formicolor = 1,
        [Display(Name = "Plut Leonardo")]
        Plut = 2,
        [Display(Name = "Lagos S.A.")]
        Lagos = 3,
        [Display(Name = "Metalúrgica Dakot S.A.")]
        Dakot = 4,
        [Display(Name = "Sande Gastón")]
        Sande = 5,
        [Display(Name = "Sin Especificar")]
        NE = 6,
    }

    public enum TexturasDeLaminados
    {
        [Display(Name="Semi Matte (MT)")]
        MT_SemiMatte = 0,
        [Display(Name = "Textura (TX)")]
        TX_Textura = 1,
        [Display(Name = "Brillante (BR)")]
        BR_Brillante = 2,
        [Display(Name = "Cotelé (CT)")]
        CT_Cotele = 3,
        [Display(Name = "Rústico (RU)")]
        RU_Rustico = 4,
        [Display(Name = "Wood Poro (WP)")]
        WP_WoodPoro = 5,
        [Display(Name = "Brush (BS)")]
        BS_Brush = 6,
        [Display(Name = "Lakan (LK)")]
        LK_Lakan = 7,
        [Display(Name = "Frost (FT)")]
        FT_Frost = 8,
        [Display(Name = "Top Matte (TM)")]
        TM_TopMatte = 9,
        [Display(Name = "Cristal (CR)")]
        CR_Cristal = 10,
        [Display(Name = "No Corresponde")]
        NC = 64,
        [Display(Name = "Sin Especificar")]
        NE = 65
    }

    public enum ColoresDeBases
    {
        Negro = 0,
        Gris = 1,
        [Display(Name = "Marrón")]
        Marron = 2,
        Blanco = 3,
        Rojo = 4,
        [Display(Name = "No Corresponde")]
        NC = 64,
        [Display(Name = "No Especifica")]
        NE = 65
    }

    public enum EspesoresDeBases
    {
        [Display(Name = "7/8\"")]
        Esp078 = 0,
        [Display(Name = "3/4\"")]
        Esp034 = 1,
        [Display(Name = "1\"")]
        Esp100 = 2,
        [Display(Name = "1 1/4\"")]
        Esp114 = 3,
        [Display(Name = "1 1/2\"")]
        Esp112 = 4,
        [Display(Name = "1 3/4\"")]
        Esp134 = 5,
        [Display(Name = "2\"")]
        Esp200 = 6,
        [Display(Name = "2 1/4\"")]
        Esp214 = 7,
        [Display(Name = "2 1/2\"")]
        Esp212 = 8,
        [Display(Name = "3\"")]
        Esp300 = 9,
        [Display(Name = "No Corresponde")]
        NC = 64,
        [Display(Name = "No Especifica")]
        NE = 65
    }

    public enum TiposDeTapas
    {
        Administrativo = 0,
        Asiento = 1,
        Cuadrada = 3,
        Libro = 4,
        [Display(Name = "Libro Redonda")]
        LibroRedonda = 5,
        Minimesa = 6,
        [Display(Name = "Pack de Minimesas")]
        PackDeMinimesas = 7,
        Octogonal = 8,
        [Display(Name = "Otro Artículos")]
        OtroArticulo = 9,
        Oval = 10,
        Plegable = 11,
        [Display(Name = "Plegable Redonda")]
        PlegableRedonda = 12,
        Posformada = 13,
        Rectangular = 15,
        Rebatible = 16,
        Redonda = 17,
        Trampa = 18,
        [Display(Name = "Trampa Redonda")]
        TrampaRedonda = 19,
        Vitrea = 21,
        [Display(Name = "Vitrea Octogonal")]
        VitreaOctogonal = 22,
        [Display(Name = "Vitrea Redonda")]
        VitreaRedonda = 23,
    }

    public enum ColoresDeBordesDeTapas
    {
        Negro = 0,
        Gris = 1,
        [Display(Name = "Marrón")]
        Marron = 2,
        Natural = 3,
        Caoba = 4,
        Blanco = 5,
        Rojo = 6,
        Ocre = 7,
        [Display(Name = "No Corresponde")]
        NC = 64,
        [Display(Name = "No Especifica")]
        NE = 65
    }

    public enum EspesoresDeBordesDeTapas
    {
        [Display(Name = "15mm")]
        Esp15 = 0,
        [Display(Name = "18mm")]
        Esp18 = 1,
        [Display(Name = "25mm")]
        Esp25 = 2,
        [Display(Name = "36mm")]
        Esp36 = 3,
        [Display(Name = "PVC Juntos")]
        PVCJuntos = 4,
        [Display(Name = "No Corresponde")]
        NC = 64,
        [Display(Name = "No Especifica")]
        NE = 65
    }

    public enum TiposDeBordesDeTapas
    {
        [Display(Name = "1pvc")]
        Borde1pvc = 0,
        [Display(Name = "2pvc")]
        Borde2pvc = 1,
        [Display(Name = "ABS")]
        BordeABS = 2,
        [Display(Name = "B/L")]
        BordeLaminado = 3,
        [Display(Name = "MDF")]
        BordeMDF = 4,
        [Display(Name = "MDFINV")]
        BordeMDFINV = 5,
        [Display(Name = "Mixto")]
        BordeMixto = 6,
        [Display(Name = "No Corresponde")]
        NC = 64,
        [Display(Name = "No Especifica")]
        NE = 65
    }

    public enum Zonas
    {
        [Display(Name = "Capital Federal Norte")]
        CABANorte = 10,
        [Display(Name = "Capital Federal Oeste")]
        CABAOeste = 11,
        [Display(Name = "Capital Federal Este")]
        CABAEste = 12,
        [Display(Name = "Capital Federal Sur")]
        CABASur = 13,
        [Display(Name = "Capital Federal Centro")]
        CABACentro = 14,
        [Display(Name = "San Martín")]
        SanMartin = 20,
        [Display(Name = "Caseros")]
        Caseros = 21,
        [Display(Name = "Morón Centro")]
        MorónCentro = 25,
        [Display(Name = "Morón Acceso")]
        MorónAcceso = 26,
        [Display(Name = "Distribuidores 1")]
        Distribuidores1 = 30,
        [Display(Name = "Distribuidores 2")]
        Distribuidores2 = 31,
        [Display(Name = "Merlo 1")]
        Merlo1 = 35,
        [Display(Name = "Merlo 2")]
        Merlo2 = 36,
        [Display(Name = "Moreno Acceso")]
        MorenoCentro = 40,
        [Display(Name = "Moreno Centro")]
        MorenoAcceso = 41,
        [Display(Name = "Tablada, San Justo")]
        SanJusto = 45,
        [Display(Name = "R. Castillo, I. Casanova")]
        RafaelCastillo = 50,
        [Display(Name = "San Miguel, Los Polvorines")]
        SanMiguel = 55,
        [Display(Name = "Tigre, San Fernando")]
        SanFernando = 60,
        [Display(Name = "Zona Desconocida")]
        ZonaDesconocida = 99,
        [Display(Name = "La Plata")]
        LaPlata = 65,
        [Display(Name = "Pilar, Del Viso, Tortuguitas")]
        Pilar = 66,
        [Display(Name = "Luján")]
        Lujan = 67
    }
}