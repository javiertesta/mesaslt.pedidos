using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pedidos.DAL;

namespace Pedidos.Models
{
    [Table("SeguimientosGlobales")]
    public class SeguimientoGlobal
    {
        
        public SeguimientoGlobal()
        {
            this.Detalle = new List<SeguimientoIndividual>();
        }

        /// <summary>
        /// Devuelve un objeto SeguimientoGlobal de acuerdo a ciertos datos de inicialización.
        /// </summary>
        /// <param name="Cantidad">Cantidad total del seguimiento. Salvo que se le encuentren otros usos, por el momento este valor coincidirá con la cantidad total del pedido asociado.</param>
        /// <param name="Usuario">Referencia al usuario logueado en el sistema.</param>
        public SeguimientoGlobal(int Cantidad, System.Security.Principal.IPrincipal Usuario) : this()
        {
            this.Detalle.Add(new SeguimientoIndividual(Cantidad, Usuario));
        }

        /// <summary>
        /// Genera una copia del Seguimiento Global actual, copiando junto con este objeto todos los objetos SeguimientoIndividual asociados.
        /// </summary>
        /// <returns>Un nueva copia del objeto SeguimientoGlobal con copia del detalle de Seguimientos Individuales.</returns>
        public SeguimientoGlobal Copiar()
        {
            var _output = new SeguimientoGlobal();
            foreach (var si in this.Detalle)
            {
                var newsi = new SeguimientoIndividual();
                newsi.Cantidad = si.Cantidad;
                newsi.Usuario = si.Usuario;
                newsi.EtapaDelNegocioInternaId = si.EtapaDelNegocioInternaId;
                newsi.Fecha = si.Fecha;
                newsi.FechaBaja = si.FechaBaja;
                _output.Detalle.Add(newsi);
            }
            return _output;
        }

        [Key, ForeignKey("Pedido")]
        [Display(Name = "Código de Pedido", ShortName = "Pedido")]
        public int SeguimientoGlobalId { get; set; }

        public virtual Pedido Pedido { get; set; }

        public DateTime FechaSeguimiento
        {
            get
            {
                return (from s in this.Detalle where s.FechaBaja == null orderby s.Fecha descending select s).FirstOrDefault().Fecha;
            }
        }

        public virtual SeguimientoIndividual ConjuntoAtrasado
        {
            get
            {
                return (from s in this.Detalle where s.FechaBaja == null orderby s.EtapaDelNegocioInterna.Nivel select s).FirstOrDefault();
            }
        }

        public virtual SeguimientoIndividual ConjuntoAdelantado
        {
            get
            {
                return (from s in this.Detalle where s.FechaBaja == null orderby s.EtapaDelNegocioInterna.Nivel descending select s).FirstOrDefault();
            }
        }

        public int CantidadPendiente
        {
            get
            {
                return this.Detalle.Where(si => si.EtapaDelNegocioInterna.Nivel < 100 && si.FechaBaja == null).AsEnumerable().Select(s => s.Cantidad).Sum();
            }
        }

        public virtual List<SeguimientoIndividual> Detalle { get; set; }

    }

    [Table("SeguimientosIndividuales")]
    public class SeguimientoIndividual
    {
        
        public SeguimientoIndividual() { }

        /// <summary>
        /// Genera un objeto de seguimiento para un subconjunto del pedido. La suma de las cantidades de los distintos Seguimientos Individuales debe igualar a la cantidad
        /// especificada en el objeto SeguimientoGlobal asociado.
        /// </summary>
        /// <param name="Cantidad">Cantidad con la que se inicializará el objeto.</param>
        /// <param name="Usuario">Referencia al usuario actual logueado en el sistema.</param>
        public SeguimientoIndividual(int Cantidad, System.Security.Principal.IPrincipal Usuario)
        {
            using (PedidosDbContext db = new PedidosDbContext())
            {
                this.Cantidad = Cantidad;
                this.Usuario = (Usuario != null || Usuario.Identity.IsAuthenticated) ? Usuario.Identity.Name : null;
                this.Fecha = DateTime.Now;
                this.EtapaDelNegocioInternaId = db.EtapasDelNegocioInternas.Where(e => e.Nivel == 0).FirstOrDefault().EtapaDelNegocioInternaId;
            }
        }

        public int SeguimientoIndividualId { get; set; }

        [ForeignKey("SeguimientoGlobal")]
        public int SeguimientoGlobalId { get; set; }

        public virtual SeguimientoGlobal SeguimientoGlobal { get; set; }
        
        public int Cantidad { get; set; }

        public DateTime Fecha { get; set; }

        public DateTime? FechaBaja { get; set; }

        public string Usuario { get; set; }

        public int EtapaDelNegocioInternaId { get; set; }

        public virtual EtapaDelNegocioInterna EtapaDelNegocioInterna { get; set; }

    }

    [Table("EtapasDelNegocioInternas")]
    public class EtapaDelNegocioInterna
    {

        public EtapaDelNegocioInterna() { }

        public int EtapaDelNegocioInternaId { get; set; }

        public string Descripcion { get; set; }

        public int Nivel { get; set; }

        public int EtapaDelNegocioPublicaId { get; set; }

        public virtual EtapaDelNegocioPublica EtapaDelNegocioPublica { get; set; }

    }

    [Table("EtapasDelNegocioPublicas")]
    public class EtapaDelNegocioPublica
    {

        public EtapaDelNegocioPublica() { }

        public int EtapaDelNegocioPublicaId { get; set; }

        public string Descripcion { get; set; }

        public int Nivel { get; set; }

    }

}