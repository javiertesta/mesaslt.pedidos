namespace Pedidos.Migrations.PedidosDbContext
{
    using Pedidos.Models;
    using Pedidos.Models.Enums;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Pedidos.DAL.PedidosDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\PedidosDbContext";
        }

        protected override void Seed(Pedidos.DAL.PedidosDbContext context)
        {

            // CLIENTES, CLIENTES, CLIENTES
            // CLIENTES, CLIENTES, CLIENTES

            context.Clientes.AddOrUpdate(new Cliente
            {
                ClienteId = "0001a",
                RazonSocial = "Mesas L.T. S.R.L.",
                Zona = Models.Enums.Zonas.ZonaDesconocida
            });

            // ETAPASDELNEGOCIOPUBLICAS, ETAPASDELNEGOCIOPUBLICAS, ETAPASDELNEGOCIOPUBLICAS
            // ETAPASDELNEGOCIOPUBLICAS, ETAPASDELNEGOCIOPUBLICAS, ETAPASDELNEGOCIOPUBLICAS

            List<EtapaDelNegocioPublica> EtapasPublicas = new List<EtapaDelNegocioPublica>();

            EtapasPublicas.Add(new EtapaDelNegocioPublica
            {
                Descripcion = "Nuevo",
                Nivel = 0,
            });

            EtapasPublicas.Add(new EtapaDelNegocioPublica
            {
                Descripcion = "Demorado",
                Nivel = 1,
            });

            EtapasPublicas.Add(new EtapaDelNegocioPublica
            {
                Descripcion = "En Proceso",
                Nivel = 2,
            });

            EtapasPublicas.Add(new EtapaDelNegocioPublica
            {
                Descripcion = "En Stock",
                Nivel = 3,
            });

            EtapasPublicas.Add(new EtapaDelNegocioPublica
            {
                Descripcion = "Despachado",
                Nivel = 4,
            });

            EtapasPublicas.Add(new EtapaDelNegocioPublica
            {
                Descripcion = "Entregado",
                Nivel = 5,
            });

            EtapasPublicas.Add(new EtapaDelNegocioPublica
            {
                Descripcion = "Baja",
                Nivel = 6,
            });

            context.EtapasDelNegocioPublicas.AddOrUpdate(enp => enp.Nivel, EtapasPublicas[0], EtapasPublicas[1], EtapasPublicas[2], EtapasPublicas[3], EtapasPublicas[4], EtapasPublicas[5], EtapasPublicas[6]);
            context.SaveChanges();

            // ETAPASDELNEGOCIOINTERNAS, ETAPASDELNEGOCIOINTERNAS, ETAPASDELNEGOCIOINTERNAS
            // ETAPASDELNEGOCIOINTERNAS, ETAPASDELNEGOCIOINTERNAS, ETAPASDELNEGOCIOINTERNAS

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 0,
                Descripcion = "Nuevo",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 0).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 1,
                Descripcion = "Demorado",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 1).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 2,
                Descripcion = "Laminado",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 0).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 3,
                Descripcion = "Aglomerado",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 0).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 4,
                Descripcion = "Tercerizado",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 2).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 5,
                Descripcion = "Cabina",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 2).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 6,
                Descripcion = "Terminación",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 2).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 7,
                Descripcion = "Stock",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 3).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 8,
                Descripcion = "Cargado",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 3).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 9,
                Descripcion = "Facturado",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 3).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 10,
                Descripcion = "En Tránsito",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 4).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 100,
                Descripcion = "Entregado",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 5).EtapaDelNegocioPublicaId
            });

            context.EtapasDelNegocioInternas.AddOrUpdate(eni => eni.Nivel, new EtapaDelNegocioInterna
            {
                Nivel = 101,
                Descripcion = "Dado de Baja",
                EtapaDelNegocioPublicaId = EtapasPublicas.Find(e => e.Nivel == 6).EtapaDelNegocioPublicaId
            });

            // MUESTRARIOS, MUESTRARIOS, MUESTRARIOS
            // MUESTRARIOS, MUESTRARIOS, MUESTRARIOS

            var NuevoMuestrario = new Muestrario
            {
                Nombre = "Mesas LT",
                Nivel = NivelesDeMuestrarios.MesasLT
            };

            context.Muestrarios.AddOrUpdate(m => m.Nombre, NuevoMuestrario);
            context.SaveChanges();

            var MuestrarioId = NuevoMuestrario.MuestrarioId;

            // LAMINADOS, LAMINADOS, LAMINADOS
            // LAMINADOS, LAMINADOS, LAMINADOS

            context.Laminados.AddOrUpdate(new Laminado
            {
                Laminado_CodigoId = "N/E",
                Laminado_MuestrarioId = NuevoMuestrario.MuestrarioId,
                Nombre = "Sin Especificar",
                Proveedor = Proveedores.NE,
                Textura = TexturasDeLaminados.NE
            });

            context.Articulos.AddOrUpdate(a => a.CodigoTango, new Tapa
            {
                Borde = new Borde
                {
                    Espesor = EspesoresDeBordesDeTapas.Esp15,
                    Tipo = TiposDeBordesDeTapas.Borde2pvc
                },
                CodigoTango = "OTRATAPA",
                Medida = null,
                Melamina = false,
                Tipo = TiposDeTapas.Rectangular
            });

            context.Articulos.AddOrUpdate(a => a.CodigoTango, new Base
            {
                CodigoTango = "OTRABASE",
                Espesor = EspesoresDeBases.Esp114,
                Proveedor = Proveedores.Dakot
            });

            context.Articulos.AddOrUpdate(a => a.CodigoTango, new Vitrea
            {
                CodigoTango = "OTRAVITREA",
                Transparente = false
            });

            base.Seed(context);
        }
    }
}
