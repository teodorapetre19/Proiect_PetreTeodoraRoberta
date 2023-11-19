using Microsoft.AspNetCore.Identity;

namespace Seminar_1.Models.Entities
{
    public class User
    {
        public User()
        {
            UserName = string.Empty;
            Password = string.Empty;
            SurName = string.Empty;
            FirstName = string.Empty;
           
        }

        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public DateTime? LastLogin { get; set; }

       
    }
}
