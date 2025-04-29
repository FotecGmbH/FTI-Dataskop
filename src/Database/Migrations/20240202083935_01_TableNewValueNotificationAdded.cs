using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class _01_TableNewValueNotificationAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewValueNotification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    NewValueNotificationType = table.Column<int>(type: "int", nullable: false),
                    AdditionalConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TblMeasurementDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    TblUserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewValueNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewValueNotification_MeasurementDefinition_TblMeasurementDefinitionId",
                        column: x => x.TblMeasurementDefinitionId,
                        principalTable: "MeasurementDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewValueNotification_User_TblUserId",
                        column: x => x.TblUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewValueNotification_TblMeasurementDefinitionId",
                table: "NewValueNotification",
                column: "TblMeasurementDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_NewValueNotification_TblUserId",
                table: "NewValueNotification",
                column: "TblUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewValueNotification");
        }
    }
}
