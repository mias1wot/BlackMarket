namespace BlackMarket_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeSliderEntity_AddSliderNumber_DeleteOrder : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Slider", new[] { "Order" });
            DropPrimaryKey("dbo.Slider");
            DropColumn("dbo.Slider", "SliderId");
            RenameColumn("dbo.Slider", "Order", "SliderNumber");
            AddPrimaryKey("dbo.Slider", "SliderNumber");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Slider");
            RenameColumn("dbo.Slider", "SliderNumber", "Order");
            AddColumn("dbo.Slider", "SliderId", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Slider", "SliderId");
            CreateIndex("dbo.Slider", "Order", unique: true);
        }
    }
}
