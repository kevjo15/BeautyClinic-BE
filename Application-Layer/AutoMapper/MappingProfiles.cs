using Application_Layer.DTO_s;
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
        }
    }
}