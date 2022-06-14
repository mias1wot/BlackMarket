namespace BlackMarket_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddColumnToProductPhotoPath : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Category", "PhotoPath", c => c.String(maxLength: 150));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Category", "PhotoPath");
        }
    }
}
