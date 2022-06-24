namespace BlackMarket_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeProductEntity_ExpandDescriptionTo500 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Product", "Description", c => c.String(nullable: false, maxLength: 500));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Product", "Description", c => c.String(nullable: false, maxLength: 250));
        }
    }
}
