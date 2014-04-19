namespace Website.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class QuotesAndTags : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Quotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        Context = c.String(),
                        AlternateAuthor = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        Author_Id = c.String(maxLength: 128),
                        Submitter_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Author_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Submitter_Id)
                .Index(t => t.Author_Id)
                .Index(t => t.Submitter_Id);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TagQuotes",
                c => new
                    {
                        Tag_Id = c.Int(nullable: false),
                        Quote_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_Id, t.Quote_Id })
                .ForeignKey("dbo.Tags", t => t.Tag_Id, cascadeDelete: true)
                .ForeignKey("dbo.Quotes", t => t.Quote_Id, cascadeDelete: true)
                .Index(t => t.Tag_Id)
                .Index(t => t.Quote_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TagQuotes", "Quote_Id", "dbo.Quotes");
            DropForeignKey("dbo.TagQuotes", "Tag_Id", "dbo.Tags");
            DropForeignKey("dbo.Quotes", "Submitter_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Quotes", "Author_Id", "dbo.AspNetUsers");
            DropIndex("dbo.TagQuotes", new[] { "Quote_Id" });
            DropIndex("dbo.TagQuotes", new[] { "Tag_Id" });
            DropIndex("dbo.Quotes", new[] { "Submitter_Id" });
            DropIndex("dbo.Quotes", new[] { "Author_Id" });
            DropTable("dbo.TagQuotes");
            DropTable("dbo.Tags");
            DropTable("dbo.Quotes");
        }
    }
}
