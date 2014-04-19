namespace Website.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InviteKeys : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InviteKeys",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(),
                        UseCount = c.Int(nullable: false, defaultValue: 0),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AspNetUsers", "Invite_Id", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "Invite_Id");
            AddForeignKey("dbo.AspNetUsers", "Invite_Id", "dbo.InviteKeys", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "Invite_Id", "dbo.InviteKeys");
            DropIndex("dbo.AspNetUsers", new[] { "Invite_Id" });
            DropColumn("dbo.AspNetUsers", "Invite_Id");
            DropTable("dbo.InviteKeys");
        }
    }
}
