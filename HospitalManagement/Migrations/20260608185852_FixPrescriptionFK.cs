using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.Migrations
{
    /// <inheritdoc />
    public partial class FixPrescriptionFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_MedicalRecords_MedicalRecordRecordId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_MedicalRecordRecordId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "MedicalRecordRecordId",
                table: "Prescriptions");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_RecordId",
                table: "Prescriptions",
                column: "RecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_MedicalRecords_RecordId",
                table: "Prescriptions",
                column: "RecordId",
                principalTable: "MedicalRecords",
                principalColumn: "RecordId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_MedicalRecords_RecordId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_RecordId",
                table: "Prescriptions");

            migrationBuilder.AddColumn<int>(
                name: "MedicalRecordRecordId",
                table: "Prescriptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicalRecordRecordId",
                table: "Prescriptions",
                column: "MedicalRecordRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_MedicalRecords_MedicalRecordRecordId",
                table: "Prescriptions",
                column: "MedicalRecordRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "RecordId");
        }
    }
}
