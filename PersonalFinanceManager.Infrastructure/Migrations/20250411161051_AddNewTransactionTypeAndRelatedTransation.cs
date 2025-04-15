using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PersonalFinanceManager.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTransactionTypeAndRelatedTransation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RelatedTransactionId",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "TransactionTypes",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 3, "Advance", "Tạm Ứng" },
                    { 4, "Repayment", "Hoàn Trả" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RelatedTransactionId",
                table: "Transactions",
                column: "RelatedTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Transactions_RelatedTransactionId",
                table: "Transactions",
                column: "RelatedTransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Transactions_RelatedTransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_RelatedTransactionId",
                table: "Transactions");

            migrationBuilder.DeleteData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "RelatedTransactionId",
                table: "Transactions");
        }
    }
}
