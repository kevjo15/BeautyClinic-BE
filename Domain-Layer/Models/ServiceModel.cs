using System;
using System.ComponentModel.DataAnnotations;

namespace Domain_Layer.Models
{
    public class ServiceModel
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public Guid? CategoryId { get; set; }
        public CategoryModel Category { get; set; }
    }
} 