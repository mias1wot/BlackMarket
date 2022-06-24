namespace BlackMarket_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeSliderEntity_AddSliderId_ClusteredIndexOnSliderNumber : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Slider");

            DropIndex("dbo.Slider", "PK_dbo.Slider");

            AddColumn("dbo.Slider", "SliderId", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Slider", "SliderId", clustered: false);
            CreateIndex("dbo.Slider", "SliderNumber", unique: true, clustered: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Slider", new[] { "SliderNumber" });
            DropPrimaryKey("dbo.Slider");
            DropColumn("dbo.Slider", "SliderId");
            AddPrimaryKey("dbo.Slider", "SliderNumber");
        }
    }
}
