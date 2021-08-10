using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.Api.Entities
{
    [Table("Locations")]
    public class Locations
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}