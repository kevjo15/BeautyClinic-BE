using Application_Layer.DTOs;
using AutoMapper;
using Domain_Layer.Models;
using Application_Layer.Commands.NotificationCommands.CreateNotification;

namespace Application_Layer.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterUserDTO, UserModel>();
            CreateMap<LoginUserDTO, UserModel>();
            CreateMap<UpdateUserProfileDTO, UserModel>();
            CreateMap<UserModel, UpdateUserProfileDTO>();
            CreateMap<UserModel, GetUserByIdResponseDTO>();
            CreateMap<ServiceModel, ServiceDTO>().ReverseMap();
            CreateMap<CategoryModel, CategoryDTO>().ReverseMap();
            CreateMap<CategoryModel, CategoryWithServicesDTO>();
            CreateMap<UserModel, UserNameDTO>();
            CreateMap<ServiceDTO, ServiceModel>().ReverseMap();
            CreateMap<BookingModel, BookingDTO>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : null))
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src =>
                    src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}".Trim() : null))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src =>
                    src.Service != null ? src.Service.Name : null))
                .ReverseMap();
            CreateMap<CreateBookingDTO, BookingModel>().ReverseMap();
            CreateMap<BookingModel, UpdateBookingDTO>().ReverseMap();
            CreateMap<MessageModel, SendMessageDTO>().ReverseMap();
            CreateMap<NotificationModel, NotificationDTO>();
            CreateMap<UserModel, EmployeeDTO>();

            // Notification mappings
            CreateMap<CreateNotificationCommand, NotificationModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false));
            // Category
            CreateMap<CategoryModel, CategoryNameDTO>();

            // Conversation / Message
            CreateMap<ConversationModel, ConversationDTO>();
            CreateMap<MessageModel, MessageDTO>();
        }
    }
}
