using ContactManager.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactManager.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260924000100_ContactManagerRequirements")]
public partial class ContactManagerRequirements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Persons",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Salutation = table.Column<string>(type: "TEXT", nullable: false),
                FirstName = table.Column<string>(type: "TEXT", nullable: false),
                LastName = table.Column<string>(type: "TEXT", nullable: false),
                DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                Gender = table.Column<bool>(type: "INTEGER", nullable: false),
                Title = table.Column<string>(type: "TEXT", nullable: false),
                SocialSecurityNumber = table.Column<string>(type: "TEXT", nullable: false),
                PhoneNumberPrivate = table.Column<string>(type: "TEXT", nullable: false),
                PhoneNumberMobile = table.Column<string>(type: "TEXT", nullable: false),
                PhoneNumberBusiness = table.Column<string>(type: "TEXT", nullable: false),
                Email = table.Column<string>(type: "TEXT", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                Nationality = table.Column<string>(type: "TEXT", nullable: false),
                Street = table.Column<string>(type: "TEXT", nullable: false),
                StreetNumber = table.Column<string>(type: "TEXT", nullable: false),
                ZipCode = table.Column<int>(type: "INTEGER", nullable: false),
                Place = table.Column<string>(type: "TEXT", nullable: false),
                PersonType = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                CompanyName = table.Column<string>(type: "TEXT", nullable: true),
                BusinessAddress = table.Column<string>(type: "TEXT", nullable: true),
                CustomerType = table.Column<char>(type: "TEXT", nullable: true),
                CompanyContact = table.Column<string>(type: "TEXT", nullable: true),
                EmployeeNumber = table.Column<Guid>(type: "TEXT", nullable: true),
                Department = table.Column<string>(type: "TEXT", nullable: true),
                StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                Employment = table.Column<int>(type: "INTEGER", nullable: true),
                Role = table.Column<string>(type: "TEXT", nullable: true),
                CadreLevel = table.Column<int>(type: "INTEGER", nullable: true),
                TraineeYears = table.Column<int>(type: "INTEGER", nullable: true),
                ActualTraineeYear = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Persons", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "CustomerContactNotes",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                NoteText = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CustomerContactNotes", x => x.Id);
                table.ForeignKey(
                    name: "FK_CustomerContactNotes_Persons_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "Persons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CustomerContactNotes_CustomerId",
            table: "CustomerContactNotes",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_Persons_EmployeeNumber",
            table: "Persons",
            column: "EmployeeNumber",
            unique: true);

        // Frühere Contact-Datensätze werden nicht gelöscht. Falls vorhanden, werden sie zusätzlich
        // als Kunden in das neue TPH-Modell übernommen. Felder, die es im alten Modell nicht gab,
        // erhalten neutrale Standardwerte und können anschliessend in der GUI ergänzt werden.
        migrationBuilder.Sql("""
            INSERT INTO Persons
            (
                Salutation, FirstName, LastName, DateOfBirth, Gender, Title,
                SocialSecurityNumber, PhoneNumberPrivate, PhoneNumberMobile,
                PhoneNumberBusiness, Email, IsActive, Nationality, Street,
                StreetNumber, ZipCode, Place, PersonType, CompanyName,
                BusinessAddress, CustomerType, CompanyContact
            )
            SELECT
                '', FirstName, LastName, '2000-01-01 00:00:00', 1, '',
                '', '', PhoneNo, '', COALESCE(Email, ''), 1, '', Address,
                '', 0, '', 'Customer', '', Address, 'E', COALESCE(Website, '')
            FROM Contacts;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CustomerContactNotes");
        migrationBuilder.DropTable(name: "Persons");
        // Die alte Contacts-Tabelle bleibt unangetastet.
    }
}
