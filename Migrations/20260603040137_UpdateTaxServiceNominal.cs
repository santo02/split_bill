using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace split_bill.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaxServiceNominal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxAmount",
                table: "Items",
                newName: "TaxValue");

            migrationBuilder.AddColumn<int>(
                name: "ServiceChargeType",
                table: "Items",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceChargeValue",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TaxType",
                table: "Items",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InputType",
                table: "ItemAssignees",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "NominalAmount",
                table: "ItemAssignees",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceChargeType",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ServiceChargeValue",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "TaxType",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "InputType",
                table: "ItemAssignees");

            migrationBuilder.DropColumn(
                name: "NominalAmount",
                table: "ItemAssignees");

            migrationBuilder.RenameColumn(
                name: "TaxValue",
                table: "Items",
                newName: "TaxAmount");
        }
    }
}
