namespace Pedidos.Migrations.PedidosDbContext
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Articulos",
                c => new
                    {
                        ArticuloId = c.Int(nullable: false, identity: true),
                        CodigoTango = c.String(),
                        Particularidades = c.String(),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Modelo = c.String(),
                        Espesor = c.Int(),
                        Color = c.Int(),
                        Proveedor = c.Int(),
                        Tipo = c.Int(),
                        Medida = c.String(),
                        Melamina = c.Boolean(),
                        Laminado_MuestrarioId = c.Int(),
                        Laminado_CodigoId = c.String(maxLength: 128),
                        Borde_Tipo = c.Int(),
                        Borde_Espesor = c.Int(),
                        Borde_Color = c.Int(),
                        Tipo1 = c.String(),
                        Medida1 = c.String(),
                        Transparente = c.Boolean(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.ArticuloId)
                .ForeignKey("dbo.Laminados", t => new { t.Laminado_MuestrarioId, t.Laminado_CodigoId })
                .Index(t => new { t.Laminado_MuestrarioId, t.Laminado_CodigoId });
            
            CreateTable(
                "dbo.Pedidos",
                c => new
                    {
                        PedidoId = c.Int(nullable: false),
                        Cantidad = c.Int(nullable: false),
                        FechaEntrega = c.DateTime(),
                        Referencia = c.String(),
                        RequiereAprobacion = c.Boolean(nullable: false),
                        EstructuraSolicitada = c.String(),
                        UserName = c.String(),
                        FechaBaja = c.DateTime(),
                        RegistroOriginalId = c.Int(),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        GestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PedidoId)
                .ForeignKey("dbo.Gestiones", t => t.GestionId, cascadeDelete: true)
                .ForeignKey("dbo.Pedidos", t => t.RegistroOriginalId)
                .ForeignKey("dbo.Articulos", t => t.PedidoId, cascadeDelete: true)
                .Index(t => t.PedidoId)
                .Index(t => t.RegistroOriginalId)
                .Index(t => t.GestionId);
            
            CreateTable(
                "dbo.Gestiones",
                c => new
                    {
                        GestionId = c.Int(nullable: false, identity: true),
                        FechaGestion = c.DateTime(nullable: false),
                        UserName = c.String(),
                        Observaciones = c.String(),
                        ClienteId = c.String(maxLength: 15),
                        FechaBaja = c.DateTime(),
                        RegistroOriginalId = c.Int(),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.GestionId)
                .ForeignKey("dbo.Clientes", t => t.ClienteId)
                .ForeignKey("dbo.Gestiones", t => t.RegistroOriginalId)
                .Index(t => t.ClienteId)
                .Index(t => t.RegistroOriginalId);
            
            CreateTable(
                "dbo.Clientes",
                c => new
                    {
                        ClienteId = c.String(nullable: false, maxLength: 15,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "CaseSensitive",
                                    new AnnotationValues(oldValue: null, newValue: "True")
                                },
                            }),
                        Zona = c.Int(nullable: false),
                        RazonSocial = c.String(),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.ClienteId);
            
            CreateTable(
                "dbo.Muestrarios",
                c => new
                    {
                        MuestrarioId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Nivel = c.Int(nullable: false),
                        ClienteId = c.String(maxLength: 15),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.MuestrarioId)
                .ForeignKey("dbo.Clientes", t => t.ClienteId, cascadeDelete: true)
                .Index(t => t.ClienteId);
            
            CreateTable(
                "dbo.Laminados",
                c => new
                    {
                        Laminado_MuestrarioId = c.Int(nullable: false),
                        Laminado_CodigoId = c.String(nullable: false, maxLength: 128),
                        Nombre = c.String(),
                        Textura = c.Int(nullable: false),
                        Proveedor = c.Int(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => new { t.Laminado_MuestrarioId, t.Laminado_CodigoId })
                .ForeignKey("dbo.Muestrarios", t => t.Laminado_MuestrarioId, cascadeDelete: true)
                .Index(t => t.Laminado_MuestrarioId);
            
            CreateTable(
                "dbo.SeguimientosGlobales",
                c => new
                    {
                        SeguimientoGlobalId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SeguimientoGlobalId)
                .ForeignKey("dbo.Pedidos", t => t.SeguimientoGlobalId, cascadeDelete: true)
                .Index(t => t.SeguimientoGlobalId);
            
            CreateTable(
                "dbo.SeguimientosIndividuales",
                c => new
                    {
                        SeguimientoIndividualId = c.Int(nullable: false, identity: true),
                        SeguimientoGlobalId = c.Int(nullable: false),
                        Cantidad = c.Int(nullable: false),
                        Fecha = c.DateTime(nullable: false),
                        FechaBaja = c.DateTime(),
                        Usuario = c.String(),
                        EtapaDelNegocioInternaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SeguimientoIndividualId)
                .ForeignKey("dbo.EtapasDelNegocioInternas", t => t.EtapaDelNegocioInternaId, cascadeDelete: true)
                .ForeignKey("dbo.SeguimientosGlobales", t => t.SeguimientoGlobalId, cascadeDelete: true)
                .Index(t => t.SeguimientoGlobalId)
                .Index(t => t.EtapaDelNegocioInternaId);
            
            CreateTable(
                "dbo.EtapasDelNegocioInternas",
                c => new
                    {
                        EtapaDelNegocioInternaId = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(),
                        Nivel = c.Int(nullable: false),
                        EtapaDelNegocioPublicaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EtapaDelNegocioInternaId)
                .ForeignKey("dbo.EtapasDelNegocioPublicas", t => t.EtapaDelNegocioPublicaId, cascadeDelete: true)
                .Index(t => t.EtapaDelNegocioPublicaId);
            
            CreateTable(
                "dbo.EtapasDelNegocioPublicas",
                c => new
                    {
                        EtapaDelNegocioPublicaId = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(),
                        Nivel = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EtapaDelNegocioPublicaId);
            
            CreateTable(
                "dbo.PedidosListados",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Fecha = c.DateTime(nullable: false),
                        Titulo = c.String(),
                        Creador = c.String(),
                        Pedidos = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Articulos", new[] { "Laminado_MuestrarioId", "Laminado_CodigoId" }, "dbo.Laminados");
            DropForeignKey("dbo.Pedidos", "PedidoId", "dbo.Articulos");
            DropForeignKey("dbo.SeguimientosGlobales", "SeguimientoGlobalId", "dbo.Pedidos");
            DropForeignKey("dbo.SeguimientosIndividuales", "SeguimientoGlobalId", "dbo.SeguimientosGlobales");
            DropForeignKey("dbo.SeguimientosIndividuales", "EtapaDelNegocioInternaId", "dbo.EtapasDelNegocioInternas");
            DropForeignKey("dbo.EtapasDelNegocioInternas", "EtapaDelNegocioPublicaId", "dbo.EtapasDelNegocioPublicas");
            DropForeignKey("dbo.Pedidos", "RegistroOriginalId", "dbo.Pedidos");
            DropForeignKey("dbo.Pedidos", "GestionId", "dbo.Gestiones");
            DropForeignKey("dbo.Gestiones", "RegistroOriginalId", "dbo.Gestiones");
            DropForeignKey("dbo.Laminados", "Laminado_MuestrarioId", "dbo.Muestrarios");
            DropForeignKey("dbo.Muestrarios", "ClienteId", "dbo.Clientes");
            DropForeignKey("dbo.Gestiones", "ClienteId", "dbo.Clientes");
            DropIndex("dbo.EtapasDelNegocioInternas", new[] { "EtapaDelNegocioPublicaId" });
            DropIndex("dbo.SeguimientosIndividuales", new[] { "EtapaDelNegocioInternaId" });
            DropIndex("dbo.SeguimientosIndividuales", new[] { "SeguimientoGlobalId" });
            DropIndex("dbo.SeguimientosGlobales", new[] { "SeguimientoGlobalId" });
            DropIndex("dbo.Laminados", new[] { "Laminado_MuestrarioId" });
            DropIndex("dbo.Muestrarios", new[] { "ClienteId" });
            DropIndex("dbo.Gestiones", new[] { "RegistroOriginalId" });
            DropIndex("dbo.Gestiones", new[] { "ClienteId" });
            DropIndex("dbo.Pedidos", new[] { "GestionId" });
            DropIndex("dbo.Pedidos", new[] { "RegistroOriginalId" });
            DropIndex("dbo.Pedidos", new[] { "PedidoId" });
            DropIndex("dbo.Articulos", new[] { "Laminado_MuestrarioId", "Laminado_CodigoId" });
            DropTable("dbo.PedidosListados");
            DropTable("dbo.EtapasDelNegocioPublicas");
            DropTable("dbo.EtapasDelNegocioInternas");
            DropTable("dbo.SeguimientosIndividuales");
            DropTable("dbo.SeguimientosGlobales");
            DropTable("dbo.Laminados");
            DropTable("dbo.Muestrarios");
            DropTable("dbo.Clientes",
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "ClienteId",
                        new Dictionary<string, object>
                        {
                            { "CaseSensitive", "True" },
                        }
                    },
                });
            DropTable("dbo.Gestiones");
            DropTable("dbo.Pedidos");
            DropTable("dbo.Articulos");
        }
    }
}
