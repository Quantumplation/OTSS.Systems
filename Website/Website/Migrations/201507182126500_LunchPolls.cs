namespace Website.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LunchPolls : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LunchOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 80),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.LunchPolls",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Decision_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LunchOptions", t => t.Decision_Id)
                .Index(t => t.Decision_Id);
            
            CreateTable(
                "dbo.LunchVotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Score = c.Int(nullable: false),
                        Option_Id = c.Int(nullable: false),
                        Poll_Id = c.Int(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LunchOptions", t => t.Option_Id, cascadeDelete: true)
                .ForeignKey("dbo.LunchPolls", t => t.Poll_Id, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Option_Id)
                .Index(t => t.Poll_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LunchVotes", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.LunchVotes", "Poll_Id", "dbo.LunchPolls");
            DropForeignKey("dbo.LunchVotes", "Option_Id", "dbo.LunchOptions");
            DropForeignKey("dbo.LunchPolls", "Decision_Id", "dbo.LunchOptions");
            DropIndex("dbo.LunchVotes", new[] { "User_Id" });
            DropIndex("dbo.LunchVotes", new[] { "Poll_Id" });
            DropIndex("dbo.LunchVotes", new[] { "Option_Id" });
            DropIndex("dbo.LunchPolls", new[] { "Decision_Id" });
            DropIndex("dbo.LunchOptions", new[] { "Name" });
            DropTable("dbo.LunchVotes");
            DropTable("dbo.LunchPolls");
            DropTable("dbo.LunchOptions");
        }
    }
}
