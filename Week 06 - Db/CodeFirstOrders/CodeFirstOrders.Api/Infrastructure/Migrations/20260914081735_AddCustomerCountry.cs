using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeFirstOrders.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF proposed AddColumn(nullable: false, defaultValue: "") — every existing customer would
            // get an empty country, and the DEFAULT constraint would let future inserts skip it too.
            // Three steps instead: add as nullable, fill the rows that exist, then make it required.
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("UPDATE Customers SET Country = N'Norway' WHERE Country IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Customers");
        }
    }
}
