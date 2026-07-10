using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure_Layer.Migrations
{
    /// <summary>
    /// Återinför den saknade FK:n Bookings.ServiceId → Services.Id.
    /// DbContext-modellen har alltid deklarerat relationen (DeleteBehavior.Restrict),
    /// men den ursprungliga Bookings-migrationen skapade aldrig constrainten, så
    /// databasen har historiskt tillåtit både föräldralösa bokningar och radering
    /// av bokade behandlingar. Rå SQL eftersom snapshoten redan tror att FK:n
    /// finns (dotnet ef genererar en tom migration). Defensiv: skapas bara om den
    /// saknas och inga föräldralösa rader finns — annars no-op istället för att
    /// fälla deployen; appen skyddar sig även i kod (ServiceRepository.DeleteServiceAsync).
    /// </summary>
    public partial class AddBookingServiceForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Bookings_Services_ServiceId')
   AND NOT EXISTS (SELECT 1 FROM Bookings b LEFT JOIN Services s ON s.Id = b.ServiceId WHERE s.Id IS NULL)
BEGIN
    ALTER TABLE Bookings WITH CHECK
        ADD CONSTRAINT FK_Bookings_Services_ServiceId
        FOREIGN KEY (ServiceId) REFERENCES Services (Id);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Bookings_Services_ServiceId')
BEGIN
    ALTER TABLE Bookings DROP CONSTRAINT FK_Bookings_Services_ServiceId;
END
");
        }
    }
}
