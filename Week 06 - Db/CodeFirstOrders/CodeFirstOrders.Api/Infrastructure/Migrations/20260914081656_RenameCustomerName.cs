using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeFirstOrders.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCustomerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Customers",
                newName: "FullName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Customers",
                newName: "Name");
        }
    }
}
