namespace Domain_Layer.Models
{
    public class CategoryModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<ServiceModel> Services { get; set; } = [];
    }
} 
