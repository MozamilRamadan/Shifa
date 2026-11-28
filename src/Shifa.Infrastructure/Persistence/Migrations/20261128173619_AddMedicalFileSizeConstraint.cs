using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shifa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalFileSizeConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Doctors");

            migrationBuilder.AlterColumn<string>(
                name: "Treatment",
                table: "MedicalRecords",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3000)",
                oldMaxLength: 3000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Prescription",
                table: "MedicalRecords",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3000)",
                oldMaxLength: 3000,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApplicationStatus",
                table: "Doctors",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRequests_PatientId_DoctorId_Status_ExpiresAt",
                table: "PermissionRequests",
                columns: new[] { "PatientId", "DoctorId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId_RecordDate",
                table: "MedicalRecords",
                columns: new[] { "PatientId", "RecordDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_MedicalFiles_FileSize",
                table: "MedicalFiles",
                sql: "\"FileSize\" >= 0 AND \"FileSize\" <= 20971520");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PermissionRequests_PatientId_DoctorId_Status_ExpiresAt",
                table: "PermissionRequests");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_PatientId_RecordDate",
                table: "MedicalRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MedicalFiles_FileSize",
                table: "MedicalFiles");

            migrationBuilder.DropColumn(
                name: "ApplicationStatus",
                table: "Doctors");

            migrationBuilder.AlterColumn<string>(
                name: "Treatment",
                table: "MedicalRecords",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Prescription",
                table: "MedicalRecords",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Doctors",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
