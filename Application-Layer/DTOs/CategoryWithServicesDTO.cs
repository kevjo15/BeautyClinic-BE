using System;
using System.Collections.Generic;
using Application_Layer.DTOs;

namespace Application_Layer.DTOs
{
    public class CategoryWithServicesDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<ServiceDTO> Services { get; set; } = [];
    }
}
