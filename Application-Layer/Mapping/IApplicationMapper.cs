using Application_Layer.Commands.NotificationCommands.CreateNotification;
using Application_Layer.DTOs;
using Domain_Layer.Models;

namespace Application_Layer.Mapping
{
    /// <summary>
    /// Objekt-till-objekt-mappning för applikationslagret. Implementeras av den
    /// Mapperly-genererade <see cref="ApplicationMapper"/> (source-generated, MIT,
    /// kompileringssäker – ersatte AutoMapper). Interfacet gör mappningen mockbar
    /// i handler-tester.
    /// </summary>
    public interface IApplicationMapper
    {
        // Bookings
        BookingDTO ToBookingDto(BookingModel booking);
        List<BookingDTO> ToBookingDtoList(List<BookingModel> bookings);
        BookingModel ToBookingModel(CreateBookingDTO dto);
        void UpdateBookingModel(UpdateBookingDTO dto, BookingModel target);

        // Services
        ServiceDTO ToServiceDto(ServiceModel service);
        List<ServiceDTO> ToServiceDtoList(IEnumerable<ServiceModel> services);
        ServiceModel ToServiceModel(ServiceDTO dto);
        void UpdateServiceModel(ServiceDTO dto, ServiceModel target);

        // Categories
        IEnumerable<CategoryNameDTO> ToCategoryNameDtos(IEnumerable<CategoryModel> categories);
        List<CategoryWithServicesDTO> ToCategoryWithServicesDtoList(IEnumerable<CategoryModel> categories);

        // Users
        UserModel ToUserModel(RegisterUserDTO dto);
        UserProfileDTO ToUserProfileDto(UserModel user);
        GetUserByIdResponseDTO ToUserByIdResponseDto(UserModel user);
        UserNameDTO ToUserNameDto(UserModel user);
        UpdateUserProfileDTO ToUpdateUserProfileDto(UserModel user);
        void UpdateUserModel(UpdateUserProfileDTO dto, UserModel target);
        List<EmployeeDTO> ToEmployeeDtoList(List<UserModel> users);

        // Conversations & messages
        ConversationDTO ToConversationDto(ConversationModel conversation);
        List<ConversationDTO> ToConversationDtoList(List<ConversationModel> conversations);
        MessageModel ToMessageModel(SendMessageDTO dto);
        List<MessageDTO> ToMessageDtoList(List<MessageModel> messages);

        // Notifications
        NotificationModel ToNotificationModel(CreateNotificationCommand command);
        NotificationDTO ToNotificationDto(NotificationModel notification);
        List<NotificationDTO> ToNotificationDtoList(List<NotificationModel> notifications);
    }
}
