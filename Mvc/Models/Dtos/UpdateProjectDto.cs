using System.ComponentModel.DataAnnotations;

namespace Mvc.Models.Dtos
{
    public class UpdateProjectDto
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; }
        public string Description { get; set; }
    }
}
