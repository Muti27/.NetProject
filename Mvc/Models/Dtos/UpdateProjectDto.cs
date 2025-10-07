using System.ComponentModel.DataAnnotations;

namespace Mvc.Models.Dtos
{
    public class UpdateProjectDto
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
