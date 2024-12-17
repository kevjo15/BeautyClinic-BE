Fullständig Beskrivning av Applikationen – Salongsbokningssystem
Projektöversikt
Applikationen är en backend-lösning för en skönhetssalong som erbjuder tjänster som fillers och botox. Den ska hantera användarprofiler, bokningar, tjänster och en livechatt-funktion mellan kunder och salongens anställda. Målet är att ge både kunder och administratörer ett effektivt och intuitivt sätt att hantera bokningar och kommunikation.

Projektet använder en Clean Architecture med CQRS och MediatR för att separera läs- och skrivoperationer. Det följer Repository Pattern för databasåtkomst och har FluentValidation kopplat till en PipelineBehavior för validering av inkommande requests.

Applikationens Funktionalitet

1. Användarhantering
   Roller: Systemet hanterar tre roller:
   Admin: Full åtkomst till alla funktioner.
   Employee: Kan hantera bokningar och kommunikation med kunder.
   Customer: Kan boka tjänster, uppdatera sin profil och kommunicera med anställda.
   Profilhantering:
   Användare kan registrera sig, logga in och uppdatera sina profiler (förnamn, efternamn och användarnamn).
   Endast administratörer kan ta bort användare.
2. Bokningssystem
   Tjänster: Admin kan skapa, uppdatera och ta bort tjänster (t.ex. fillers, botox) med tillgängliga tider.
   Lediga tider: Kunder kan se en kalender med tillgängliga tider för en viss tjänst och boka en tid.
   Bokning: Kunder kan boka, uppdatera och avboka tider.
   Admin-åtkomst: Administratörer kan se och hantera alla bokningar.
3. Livechatt
   Realtidskommunikation: SignalR används för livechatt mellan kunder och anställda.
   Meddelanden: Kunder kan kommunicera med den anställda som hanterar deras bokning.
   Admins: Administratörer kan delta i eller granska alla konversationer.
4. Autentisering och Autorisering
   JWT-baserad autentisering: Alla endpoints kräver JWT-token för åtkomst.
   Rollbaserad autorisering: Endpoints är begränsade baserat på användarroll.
   Uppdatera token: Stöd för refresh-tokens för att förlänga användarsessioner.
   Endpoints
   Användarhantering
   POST /api/users/register
   Registrerar en ny användare.
   POST /api/users/login
   Loggar in en användare och returnerar en JWT-token.
   PUT /api/users/me/update-profile
   Uppdaterar den inloggade användarens profil.
   GET /api/users/{id}
   Hämtar en specifik användare baserat på ID (Admin-only).
   Bokningar
   GET /api/bookings/available-slots
   Hämtar tillgängliga tider baserat på tjänst och datum.
   POST /api/bookings
   Skapar en ny bokning.
   GET /api/bookings/my-bookings
   Hämtar alla bokningar för den inloggade användaren.
   PUT /api/bookings/{id}
   Uppdaterar en bokning.
   DELETE /api/bookings/{id}
   Avbokar en bokning.
   Tjänster
   GET /api/services
   Hämtar en lista över alla tjänster.
   POST /api/services
   Skapar en ny tjänst (Admin-only).
   PUT /api/services/{id}
   Uppdaterar en tjänst (Admin-only).
   DELETE /api/services/{id}
   Tar bort en tjänst (Admin-only).
   Livechatt
   POST /api/chat/send-message
   Skickar ett meddelande i en konversation.
   GET /api/chat/conversations
   Hämtar alla konversationer för en användare.
   GET /api/chat/messages/{conversationId}
   Hämtar alla meddelanden i en specifik konversation.
   Backend Arkitektur
   Verktyg och Bibliotek
   ASP.NET Core: För API-utveckling.
   Entity Framework Core: För databasåtkomst.
   MediatR: För CQRS-mönster.
   FluentValidation: För validering av inkommande requests.
   SignalR: För realtidskommunikation.
   AutoMapper: För att mappa mellan modeller och DTO:er.
   JWT Autentisering: För säker API-åtkomst.
   Swagger: För dokumentation och testning av API:et.
   Validering
   FluentValidation
   Användarhantering:

E-post: Valideras med EmailValidationExtension.
Namn: Valideras med NameValidationExtension.
Lösenord: Valideras med PasswordValidationExtension.
Användarnamn: Valideras med UserNameValidationExtension.
Globala ID:n:
Används för att validera GUID-baserade ID:n för användare, bokningar och tjänster.

Plan för Implementation av Refresh Tokens
Skapa en Refresh Token-modell och lagra token i databasen.
Lägg till endpoints:
POST /api/users/refresh-token: För att uppdatera access-token.
POST /api/users/revoke-token: För att återkalla en refresh-token.
Lägg till middleware för att validera och hantera refresh-tokens.
Datamodeller
UserModel
csharp
Kopiera kod
public class UserModel : IdentityUser
{
public string FirstName { get; set; }
public string LastName { get; set; }
public List<Booking> Bookings { get; set; }
public List<Conversation> Conversations { get; set; }
}
Booking
csharp
Kopiera kod
public class Booking
{
public Guid BookingId { get; set; }
public string CustomerId { get; set; }
public string ServiceId { get; set; }
public DateTime StartTime { get; set; }
public DateTime EndTime { get; set; }
}
Conversation
csharp
Kopiera kod
public class Conversation
{
public Guid ConversationId { get; set; }
public string CustomerId { get; set; }
public string EmployeeId { get; set; }
public List<Message> Messages { get; set; }
}
Message
csharp
Kopiera kod
public class Message
{
public Guid MessageId { get; set; }
public string SenderId { get; set; }
public string Content { get; set; }
public DateTime Timestamp { get; set; }
}
Fullständiga Mål
För kunder:

Enkel bokning av tjänster.
Realtidskommunikation med salongen.
För administratörer:

Hantera tjänster, bokningar och användare effektivt.
Övervaka och delta i kundkonversationer.
För anställda:

Hantera bokningar och svara på kunders frågor via livechatt.
