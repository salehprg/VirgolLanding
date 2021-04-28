using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace virgollanding.Migrations
{
    public partial class DbUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "ReqForms",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeRequest",
                table: "ReqForms",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "ReqForms");

            migrationBuilder.DropColumn(
                name: "TimeRequest",
                table: "ReqForms");
        }
    }
}
