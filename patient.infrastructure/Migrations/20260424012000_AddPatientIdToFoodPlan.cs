using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using patient.infrastructure.Percistence.DomainModel;

#nullable disable

namespace patient.infrastructure.Migrations
{
    [DbContext(typeof(DomainDbContext))]
    [Migration("20260424012000_AddPatientIdToFoodPlan")]
    public partial class AddPatientIdToFoodPlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "FoodPlan",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "FoodPlan");
        }
    }
}
