namespace Pedidos.Migrations.PedidosDbContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FueraDeLista : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Articulos", "Titulo", c => c.String());
            AddColumn("dbo.Articulos", "Detalle", c => c.String());
            AddColumn("dbo.Pedidos", "Observaciones", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Pedidos", "Observaciones");
            DropColumn("dbo.Articulos", "Detalle");
            DropColumn("dbo.Articulos", "Titulo");
        }
    }
}
