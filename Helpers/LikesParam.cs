namespace DatingApp.Api.Helpers
{
    public class LikesParam : PaginationParams
    {
        public string Predicate { get; set; }
        public int UserId { get; set; }
    }
}