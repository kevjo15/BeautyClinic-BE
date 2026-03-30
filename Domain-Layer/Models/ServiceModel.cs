using System.ComponentModel.DataAnnotations;

namespace Domain_Layer.Models
{
    public class ServiceModel
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public CategoryModel? Category { get; set; }
    }
}
