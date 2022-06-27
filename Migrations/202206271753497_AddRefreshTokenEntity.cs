namespace BlackMarket_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRefreshTokenEntity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RefreshToken",
                c => new
                    {
                        RefreshTokenId = c.String(nullable: false, maxLength: 128),
                        UserId = c.Long(nullable: false),
                        ClientId = c.Int(nullable: false),
                        Ticket = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.RefreshTokenId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RefreshToken");
        }
    }
}
