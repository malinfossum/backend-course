using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeFirstOrders.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCustomerEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Customers",
                newName: "EmailAddress");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                newName: "IX_Customers_EmailAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailAddress",
                table: "Customers",
                newName: "Email");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_EmailAddress",
                table: "Customers",
                newName: "IX_Customers_Email");
        }
    }
}
