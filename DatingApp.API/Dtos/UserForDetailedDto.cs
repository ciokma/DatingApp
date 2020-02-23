using System;
using System.Collections.Generic;
using DatingApp.API.Data;

namespace DatingApp.API.Dtos
{
    public class UserForDetailedDto
    {
        public int id { get; set; }
        public string userName  { get; set; } 
        public string gender { get; set; }
        public int age { get; set; }
        public string knownAs { get; set; }
        public DateTime created { get; set; }
        public DateTime lastActivity { get; set; }
        public string introduction { get; set; }
        public string lookingFor { get; set; }
        public string interests { get; set; }
        public string city { get; set; }
        public string country {get; set; }
        public string photoUrl { get; set; }
        public ICollection<PhotosForDetailedDto> photos { get; set; }
    }
}