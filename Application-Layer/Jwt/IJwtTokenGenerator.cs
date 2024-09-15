using Domain_Layer.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Jwt
{
    public interface IJwtTokenGenerator
    {
        Task<string> GenerateToken(UserModel user);
    }
}
