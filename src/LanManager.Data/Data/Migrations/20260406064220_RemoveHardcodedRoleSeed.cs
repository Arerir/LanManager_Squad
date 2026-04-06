using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LanManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHardcodedRoleSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "10000000-0000-0000-0000-000000000001", "Admin", "ADMIN" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "10000000-0000-0000-0000-000000000002", "Organizer", "ORGANIZER" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "10000000-0000-0000-0000-000000000003", "Attendee", "ATTENDEE" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "10000000-0000-0000-0000-000000000004", "Operator", "OPERATOR" }
                });
        }
    }
}
