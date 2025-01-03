﻿using Application_Layer.DTO_s;
using Application_Layer.DTOs;
using ApplicationLayer.DTOs;
using AutoMapper;
using Domain_Layer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            CreateMap<ServiceDTO, ServiceModel>().ReverseMap();
            CreateMap<BookingModel, BookingDTO>().ReverseMap();
            CreateMap<CreateBookingDTO, BookingModel>().ReverseMap();
            CreateMap<BookingModel, UpdateBookingDTO>().ReverseMap();
            CreateMap<MessageModel, SendMessageDTO>().ReverseMap();
        }
    }
}
