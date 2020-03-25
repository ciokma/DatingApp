using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers
{
    public class PageListed<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; } 
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public PageListed(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize  = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double) pageSize);
            this.AddRange(items);
        }
        ///realizar el paginado
        public static async Task<PageListed<T>> CreateAsync(IQueryable<T> source, 
        int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1 ) * pageSize).Take(pageSize).ToListAsync();
            return new PageListed<T>(items, count, pageNumber, pageSize);
        }
    }
}