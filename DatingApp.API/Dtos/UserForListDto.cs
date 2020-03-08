using System;

namespace DatingApp.API.Dtos
{
    public class UserForListDto
    {
        public int id { get; set; }
        public string userName  { get; set; }
        /*public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        */
        public string gender { get; set; }
        public int age { get; set; }
        public string knownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime lastActivity { get; set; }
        public string city { get; set; }
        public string country {get; set; }
        public string photoUrl { get; set; }
    }
}