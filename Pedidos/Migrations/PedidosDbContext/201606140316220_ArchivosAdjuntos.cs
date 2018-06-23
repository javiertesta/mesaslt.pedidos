namespace Pedidos.Migrations.PedidosDbContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArchivosAdjuntos : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ArchivosEnDisco",
                c => new
                    {
                        ArchivoEnDiscoId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(maxLength: 255),
                        NombreOriginal = c.String(maxLength: 255),
                        FechaCreacion = c.DateTime(nullable: false),
                        Ubicacion = c.String(),
                        Tipo = c.String(),
                        PedidoId = c.Int(),
                    })
                .PrimaryKey(t => t.ArchivoEnDiscoId)
                .ForeignKey("dbo.Pedidos", t => t.PedidoId)
                .Index(t => t.PedidoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ArchivosEnDisco", "PedidoId", "dbo.Pedidos");
            DropIndex("dbo.ArchivosEnDisco", new[] { "PedidoId" });
            DropTable("dbo.ArchivosEnDisco");
        }
    }
}
