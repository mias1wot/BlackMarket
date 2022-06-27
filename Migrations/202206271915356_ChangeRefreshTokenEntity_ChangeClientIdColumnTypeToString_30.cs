namespace BlackMarket_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeRefreshTokenEntity_ChangeClientIdColumnTypeToString_30 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RefreshToken", "ClientId", c => c.String(nullable: false, maxLength: 30));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RefreshToken", "ClientId", c => c.Int(nullable: false));
        }
    }
}
