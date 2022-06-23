namespace BlackMarket_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSliderEntity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Slider",
                c => new
                    {
                        SliderId = c.Int(nullable: false, identity: true),
                        Order = c.Int(nullable: false),
                        PhotoPath = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.SliderId)
                .Index(t => t.Order, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Slider", new[] { "Order" });
            DropTable("dbo.Slider");
        }
    }
}
