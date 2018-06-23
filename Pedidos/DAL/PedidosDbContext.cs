using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using Pedidos.Models;

namespace Pedidos.DAL
{

    public partial class PedidosDbContext : DbContext
    {
        
        public PedidosDbContext()
            : base("name=DefaultConnection")
        {
            Database.SetInitializer<PedidosDbContext>(null);
            Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Gestion>().HasOptional(c => c.RegistroOriginal);
            modelBuilder.Entity<Gestion>().HasMany(c => c.Historial).WithOptional(c => c.RegistroOriginal).HasForeignKey(c => c.RegistroOriginalId);
            modelBuilder.Entity<Pedido>().HasOptional(p => p.RegistroOriginal);
            modelBuilder.Entity<Pedido>().HasMany(p => p.Historial).WithOptional(p => p.RegistroOriginal).HasForeignKey(p => p.RegistroOriginalId);
            modelBuilder.Entity<Pedido>().HasOptional(p => p.SeguimientoGlobal).WithRequired(sg => sg.Pedido).WillCascadeOnDelete(true);
            modelBuilder.Entity<SeguimientoGlobal>().HasMany(sg => sg.Detalle).WithRequired(si => si.SeguimientoGlobal).WillCascadeOnDelete(true);
            modelBuilder.Entity<Articulo>().HasOptional(a => a.Pedido).WithRequired(p => p.Articulo).WillCascadeOnDelete(true);
            modelBuilder.Entity<Cliente>().Property(p => p.ClienteId).HasColumnAnnotation("CaseSensitive", true);
            modelBuilder.Entity<Muestrario>().HasOptional(m => m.Cliente).WithMany(c => c.Muestrarios).HasForeignKey(m => m.ClienteId).WillCascadeOnDelete();
            
            modelBuilder.Entity<Tapa>()
                .HasOptional(t => t.Laminado)
                .WithMany()
                .HasForeignKey(t => new { t.Laminado_MuestrarioId, t.Laminado_CodigoId });            

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<SeguimientoGlobal> SeguimientosGlobales { get; set; }
        public virtual DbSet<SeguimientoIndividual> SeguimientosIndividuales { get; set; }
        public virtual DbSet<EtapaDelNegocioInterna> EtapasDelNegocioInternas { get; set; }
        public virtual DbSet<EtapaDelNegocioPublica> EtapasDelNegocioPublicas { get; set; }
        public virtual DbSet<Articulo> Articulos { get; set; }
        public virtual DbSet<Laminado> Laminados { get; set; }
        public virtual DbSet<Muestrario> Muestrarios { get; set; }
        public virtual DbSet<Gestion> Gestiones { get; set; }
        public virtual DbSet<Pedido> Pedidos { get; set; }
        public virtual DbSet<Cliente> Clientes { get; set; }
        public virtual DbSet<PedidoListado> PedidosListados { get; set; }
        public virtual DbSet<ArchivoEnDisco> ArchivosEnDisco { get; set; }

    }

}