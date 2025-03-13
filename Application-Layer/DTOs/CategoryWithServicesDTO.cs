using System;
using System.Collections.Generic;
using Application_Layer.DTO_s;

namespace Application_Layer.DTOs
{
    public class CategoryWithServicesDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<ServiceDTO> Services { get; set; }
    }
}
