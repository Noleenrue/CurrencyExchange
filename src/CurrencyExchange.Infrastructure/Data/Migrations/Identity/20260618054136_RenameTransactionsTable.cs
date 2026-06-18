using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CurrencyExchange.Infrastructure.Data.Migrations.Identity
{
    /// <inheritdoc />
    public partial class RenameTransactionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_UserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Currencies_CurrencyId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "ExchangeTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserId",
                table: "ExchangeTransactions",
                newName: "IX_ExchangeTransactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CurrencyId",
                table: "ExchangeTransactions",
                newName: "IX_ExchangeTransactions_CurrencyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExchangeTransactions",
                table: "ExchangeTransactions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeTransactions_AspNetUsers_UserId",
                table: "ExchangeTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeTransactions_Currencies_CurrencyId",
                table: "ExchangeTransactions",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeTransactions_AspNetUsers_UserId",
                table: "ExchangeTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeTransactions_Currencies_CurrencyId",
                table: "ExchangeTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExchangeTransactions",
                table: "ExchangeTransactions");

            migrationBuilder.RenameTable(
                name: "ExchangeTransactions",
                newName: "Transactions");

            migrationBuilder.RenameIndex(
                name: "IX_ExchangeTransactions_UserId",
                table: "Transactions",
                newName: "IX_Transactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ExchangeTransactions_CurrencyId",
                table: "Transactions",
                newName: "IX_Transactions_CurrencyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Currencies_CurrencyId",
                table: "Transactions",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
