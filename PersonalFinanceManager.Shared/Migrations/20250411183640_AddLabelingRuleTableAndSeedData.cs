using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PersonalFinanceManager.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLabelingRuleTableAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabelingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Keyword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionTypeId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelingRules", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LabelingRules",
                columns: new[] { "Id", "CategoryId", "Keyword", "TransactionTypeId" },
                values: new object[,]
                {
                    { 1, 3, "KOVQR", 2 },
                    { 2, 5, "LE NGUYEN TUAN chuyen khoan", 2 },
                    { 3, 3, "603 - 60.7 UVK", 3 },
                    { 4, 4, "tien cau long", 2 },
                    { 5, 3, "Tien Banh Mi", 2 },
                    { 6, 3, "Tien com toi", 2 },
                    { 7, 3, "Tien com trua", 2 },
                    { 8, 5, "GT-PS", 2 },
                    { 9, 4, "HT-PT", 2 },
                    { 10, 3, "SH - ", 2 },
                    { 11, 1, "Lương tháng", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabelingRules");
        }
    }
}
