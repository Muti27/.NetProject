using System.ComponentModel.DataAnnotations;

namespace Mvc.Dtos
{
    public class UpdateProjectDto
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
