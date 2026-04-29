using System.ComponentModel.DataAnnotations;

namespace Admin.API.Models
{
    public class Airport
    {
        [Key] public int Id { get; set; }
        [Required] public string Code { get; set; } = string.Empty;
        [Required] public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class Airline
    {
        [Key] public int Id { get; set; }
        [Required] public string Code { get; set; } = string.Empty;
        [Required] public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
