namespace AuthWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullableOrder : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "dataEntrega", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "dataEntrega", c => c.DateTime(nullable: false));
        }
    }
}
