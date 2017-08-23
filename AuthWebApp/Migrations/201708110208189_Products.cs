namespace AuthWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Products : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        nome = c.String(nullable: false),
                        descricao = c.String(),
                        cor = c.String(),
                        modelo = c.String(nullable: false, maxLength: 450),
                        codigo = c.String(nullable: false, maxLength: 450),
                        preco = c.Decimal(nullable: false, precision: 18, scale: 2),
                        peso = c.Decimal(nullable: false, precision: 18, scale: 2),
                        altura = c.Decimal(nullable: false, precision: 18, scale: 2),
                        largura = c.Decimal(nullable: false, precision: 18, scale: 2),
                        comprimento = c.Decimal(nullable: false, precision: 18, scale: 2),
                        diametro = c.Decimal(nullable: false, precision: 18, scale: 2),
                        url = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.modelo, unique: true)
                .Index(t => t.codigo, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Products", new[] { "codigo" });
            DropIndex("dbo.Products", new[] { "modelo" });
            DropTable("dbo.Products");
        }
    }
}
