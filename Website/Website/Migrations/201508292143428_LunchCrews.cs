namespace Website.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LunchCrews : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserLunchPolls",
                c => new
                    {
                        User_Id = c.String(nullable: false, maxLength: 128),
                        LunchPoll_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.LunchPoll_Id })
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.LunchPolls", t => t.LunchPoll_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.LunchPoll_Id);

            Sql("INSERT INTO dbo.UserLunchPolls(User_Id, LunchPoll_Id)\n" +
                "SELECT DISTINCT User_Id, Poll_Id\n" +
                "FROM dbo.LunchVotes");

            AddColumn("dbo.LunchPolls", "Name", c => c.String(nullable: true));
            Sql("UPDATE dbo.LunchPolls SET Name = 'Luncharoo' WHERE Name IS NULL");
            AlterColumn("dbo.LunchPolls", "Name", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserLunchPolls", "LunchPoll_Id", "dbo.LunchPolls");
            DropForeignKey("dbo.UserLunchPolls", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.UserLunchPolls", new[] { "LunchPoll_Id" });
            DropIndex("dbo.UserLunchPolls", new[] { "User_Id" });
            DropColumn("dbo.LunchPolls", "Name");
            DropTable("dbo.UserLunchPolls");
        }
    }
}
