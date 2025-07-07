using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGarageSale.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseDescriptionLengthTo4000 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE Items ALTER COLUMN Description NVARCHAR(4000) NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE Items ALTER COLUMN Description NVARCHAR(1000) NOT NULL;");
        }
    }
}
