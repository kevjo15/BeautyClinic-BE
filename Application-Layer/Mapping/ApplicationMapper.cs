using Application_Layer.Commands.NotificationCommands.CreateNotification;
using Application_Layer.DTOs;
using Domain_Layer.Models;
using Riok.Mapperly.Abstractions;

namespace Application_Layer.Mapping
{
    /// <summary>
    /// Source-generated mappning (Riok.Mapperly). Ersatte AutoMapper: MIT-licensierad,
    /// kompileringssäker och utan runtime-reflection. Triviala 1:1- och list-mappningar
    /// genereras av Mapperly; mappningar med egen logik är handskrivna nedan.
    /// </summary>
    [Mapper]
    public partial class ApplicationMapper : IApplicationMapper
    {
        // ---- Bookings ------------------------------------------------------

        /// <summary>Platta namnfält + nästlade User/Employee, som tidigare AutoMapper-profil.</summary>
        public BookingDTO ToBookingDto(BookingModel b) => new()
        {
            Id = b.Id,
            UserId = b.UserId,
            ServiceId = b.ServiceId,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            EmployeeId = b.EmployeeId,
            ConversationId = b.ConversationId,
            CustomerName = FullName(b.User),
            EmployeeName = FullName(b.Employee),
            ServiceName = b.Service?.Name,
            Status = b.Status.ToString(),
            HasSavedCard = !string.IsNullOrWhiteSpace(b.StripePaymentMethodId),
            CardBrand = b.CardBrand,
            CardLast4 = b.CardLast4,
            PaymentStatus = b.PaymentStatus.ToString(),
            AmountPaid = b.AmountPaid,
            User = ToBookingUserDto(b.User),
            Employee = ToBookingUserDto(b.Employee),
        };

        public partial List<BookingDTO> ToBookingDtoList(List<BookingModel> bookings);

        [MapperIgnoreTarget(nameof(BookingModel.Id))]
        [MapperIgnoreTarget(nameof(BookingModel.ConversationId))]
        [MapperIgnoreTarget(nameof(BookingModel.Status))]
        [MapperIgnoreTarget(nameof(BookingModel.ReminderSentAt))]
        [MapperIgnoreTarget(nameof(BookingModel.StripePaymentMethodId))]
        [MapperIgnoreTarget(nameof(BookingModel.CardBrand))]
        [MapperIgnoreTarget(nameof(BookingModel.CardLast4))]
        [MapperIgnoreTarget(nameof(BookingModel.NoShowFeeChargedAt))]
        [MapperIgnoreTarget(nameof(BookingModel.PaymentStatus))]
        [MapperIgnoreTarget(nameof(BookingModel.AmountPaid))]
        [MapperIgnoreTarget(nameof(BookingModel.StripePaymentIntentId))]
        [MapperIgnoreTarget(nameof(BookingModel.User))]
        [MapperIgnoreTarget(nameof(BookingModel.Employee))]
        [MapperIgnoreTarget(nameof(BookingModel.Service))]
        public partial BookingModel ToBookingModel(CreateBookingDTO dto);

        [MapperIgnoreTarget(nameof(BookingModel.Id))]
        [MapperIgnoreTarget(nameof(BookingModel.UserId))]
        [MapperIgnoreTarget(nameof(BookingModel.ConversationId))]
        [MapperIgnoreTarget(nameof(BookingModel.Status))]
        [MapperIgnoreTarget(nameof(BookingModel.ReminderSentAt))]
        [MapperIgnoreTarget(nameof(BookingModel.StripePaymentMethodId))]
        [MapperIgnoreTarget(nameof(BookingModel.CardBrand))]
        [MapperIgnoreTarget(nameof(BookingModel.CardLast4))]
        [MapperIgnoreTarget(nameof(BookingModel.NoShowFeeChargedAt))]
        [MapperIgnoreTarget(nameof(BookingModel.PaymentStatus))]
        [MapperIgnoreTarget(nameof(BookingModel.AmountPaid))]
        [MapperIgnoreTarget(nameof(BookingModel.StripePaymentIntentId))]
        [MapperIgnoreTarget(nameof(BookingModel.User))]
        [MapperIgnoreTarget(nameof(BookingModel.Employee))]
        [MapperIgnoreTarget(nameof(BookingModel.Service))]
        public partial void UpdateBookingModel(UpdateBookingDTO dto, BookingModel target);

        private static string? FullName(UserModel? u) =>
            u == null ? null : $"{u.FirstName} {u.LastName}".Trim();

        private static BookingUserDTO? ToBookingUserDto(UserModel? u) =>
            u == null ? null : new BookingUserDTO { FirstName = u.FirstName, LastName = u.LastName, Email = u.Email };

        // ---- Services ------------------------------------------------------

        public partial ServiceDTO ToServiceDto(ServiceModel service);
        public partial List<ServiceDTO> ToServiceDtoList(IEnumerable<ServiceModel> services);

        // Id ignoreras: identiteten ägs av URL:en/entiteten, aldrig av body:n —
        // klienter skickar inte id i DTO:n, och att mappa dto.Id (Guid.Empty)
        // över en spårad entitet korrumperar nyckeln.
        // ImageUrl ignoreras: databasen lagrar blob-path, men DTO:n bär en färsk
        // SAS-URL vid läsning. Mappas den tillbaka vid create/update skulle en
        // SAS-URL lagras (bryter mot "aldrig lagrad SAS"). Bilden ändras enbart
        // via POST /api/services/{id}/image, som lagrar blob-path.
        [MapperIgnoreTarget(nameof(ServiceModel.Id))]
        [MapperIgnoreTarget(nameof(ServiceModel.ImageUrl))]
        [MapperIgnoreTarget(nameof(ServiceModel.CategoryId))]
        [MapperIgnoreTarget(nameof(ServiceModel.Category))]
        public partial ServiceModel ToServiceModel(ServiceDTO dto);

        [MapperIgnoreTarget(nameof(ServiceModel.Id))]
        [MapperIgnoreTarget(nameof(ServiceModel.ImageUrl))]
        [MapperIgnoreTarget(nameof(ServiceModel.CategoryId))]
        [MapperIgnoreTarget(nameof(ServiceModel.Category))]
        public partial void UpdateServiceModel(ServiceDTO dto, ServiceModel target);

        // ---- Categories ----------------------------------------------------

        public partial IEnumerable<CategoryNameDTO> ToCategoryNameDtos(IEnumerable<CategoryModel> categories);
        public partial List<CategoryWithServicesDTO> ToCategoryWithServicesDtoList(IEnumerable<CategoryModel> categories);

        // ---- Users ---------------------------------------------------------

        [MapperIgnoreTarget(nameof(UserModel.Id))]
        [MapperIgnoreTarget(nameof(UserModel.UserName))]
        [MapperIgnoreTarget(nameof(UserModel.IsDeleted))]
        [MapperIgnoreTarget(nameof(UserModel.AvatarUrl))]
        [MapperIgnoreTarget(nameof(UserModel.EmailConfirmed))]
        public partial UserModel ToUserModel(RegisterUserDTO dto);

        /// <summary>UserId speglar Id; Role/AvatarUrl sätts av anroparen.</summary>
        public UserProfileDTO ToUserProfileDto(UserModel u) => new()
        {
            UserId = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            PhoneNumber = u.PhoneNumber,
            AvatarUrl = u.AvatarUrl,
        };

        public partial GetUserByIdResponseDTO ToUserByIdResponseDto(UserModel user);
        public partial UserNameDTO ToUserNameDto(UserModel user);
        public partial UpdateUserProfileDTO ToUpdateUserProfileDto(UserModel user);

        [MapperIgnoreTarget(nameof(UserModel.Id))]
        [MapperIgnoreTarget(nameof(UserModel.UserName))]
        [MapperIgnoreTarget(nameof(UserModel.Email))]
        [MapperIgnoreTarget(nameof(UserModel.IsDeleted))]
        [MapperIgnoreTarget(nameof(UserModel.AvatarUrl))]
        [MapperIgnoreTarget(nameof(UserModel.EmailConfirmed))]
        public partial void UpdateUserModel(UpdateUserProfileDTO dto, UserModel target);

        public partial List<EmployeeDTO> ToEmployeeDtoList(List<UserModel> users);

        // ---- Conversations & messages -------------------------------------

        public partial ConversationDTO ToConversationDto(ConversationModel conversation);
        public partial List<ConversationDTO> ToConversationDtoList(List<ConversationModel> conversations);

        /// <summary>Id kopieras som tidigare (null → Guid.Empty).</summary>
        public MessageModel ToMessageModel(SendMessageDTO dto) => new()
        {
            Id = dto.Id.GetValueOrDefault(),
            ConversationId = dto.ConversationId,
            SenderId = dto.SenderId,
            Content = dto.Content,
            SentAt = dto.SentAt,
            ReadAt = dto.ReadAt,
        };

        public partial List<MessageDTO> ToMessageDtoList(List<MessageModel> messages);

        // ---- Notifications -------------------------------------------------

        /// <summary>Nytt Id + oläst; CreatedAt sätts av anroparen.</summary>
        public NotificationModel ToNotificationModel(CreateNotificationCommand c) => new()
        {
            Id = Guid.NewGuid(),
            Title = c.Title,
            Message = c.Message,
            UserId = c.UserId,
            Type = c.Type,
            BookingId = c.BookingId,
            ConversationId = c.ConversationId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        };

        /// <summary>CreatedAt tolkas som UTC och exponeras som DateTimeOffset.</summary>
        public NotificationDTO ToNotificationDto(NotificationModel n) => new()
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(n.CreatedAt, DateTimeKind.Utc)),
            IsRead = n.IsRead,
            Type = n.Type,
            BookingId = n.BookingId,
            UserId = n.UserId,
            ConversationId = n.ConversationId,
        };

        public partial List<NotificationDTO> ToNotificationDtoList(List<NotificationModel> notifications);
    }
}
