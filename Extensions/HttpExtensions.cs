using System.Text.Json;
using DatingApp.Api.Helpers;
using Microsoft.AspNetCore.Http;

namespace DatingApp.Api.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int currentPage,
                                    int itemPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage,itemPerPage,totalItems,totalPages);
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader, options));
            response.Headers.Add("Access-Control-Expose-Headers","Pagination");
        }                                    
    }
}