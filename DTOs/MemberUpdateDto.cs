namespace DatingApp.Api.DTOs
{
    public class MemberUpdateDto
    {
        public LocationDto[] Locations { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
    }
}