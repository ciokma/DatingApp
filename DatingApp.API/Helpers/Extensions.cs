using System;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origen","*");
        }
        //paginacion
        public static void AddPagination(this HttpResponse response, 
        int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader =  new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            var camelCaseFormater =  new JsonSerializerSettings();
            camelCaseFormater.ContractResolver = new CamelCasePropertyNamesContractResolver();
            
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormater));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");

        }
        //agregando una extension para calcular la edad
        public static int CalculateAge(this DateTime theDateTime)
        {
            var age =  DateTime.Today.Year - theDateTime.Year;
            if(theDateTime.AddYears(age) > DateTime.Now)
                age--;

            return age;
        }
         // Summary:
        //     Resolve destination member using a custom value resolver callback. Used instead
        //     of MapFrom when not simply redirecting a source member This method cannot be
        //     used in conjunction with LINQ query projection
        //
        // Parameters:
        //   resolver:
        //     Callback function to resolve against source type
        //ESTA EXTENSION NO ME SIRVIO
        public static void ResolveUsing<TSource, TDestination, TMember, TResult>(this IMemberConfigurationExpression<TSource, TDestination, TMember> member, Func<TSource, TResult> resolver) => member.MapFrom((Func<TSource, TDestination, TResult>)((src, dest) => resolver(src)));
    }
}