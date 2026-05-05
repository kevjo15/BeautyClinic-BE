using System;

namespace Application_Layer.DTOs
{
    public class ServiceDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
