using Seminar_1.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Seminar_1.Models.VMs
{
    public class UserVM
    {
        public UserVM()
        {
            UserName = string.Empty;
            Password = string.Empty;
            SurName = string.Empty;
            FirstName = string.Empty;

        }

        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(256)]
        public string UserName { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(256)]
        public string Password { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(256)]
        public string SurName { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(256)]
        public string FirstName { get; set; }
        public DateTime? LastLogin { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(256)]
        public string ConfirmPassword { get; set; }

        public static User VMPUserToUser(UserVM vm)
        {
            var user = new User();

            user.UserName = vm.UserName;
            user.Password = vm.Password;
            user.SurName = vm.SurName;
            user.FirstName = vm.FirstName;
           

            return user;
        }

        public UserVM UserToUserVM(User? user)
        {
            if (user == null)
                return new UserVM();

            var vm = new UserVM();

            vm.Id = user.Id;
            vm.UserName = user.UserName;
            vm.Password = user.Password;
            vm.SurName = user.SurName;
            vm.FirstName = user.FirstName;
            

            return vm;
        }

        private readonly Seminar1Context _context;
        public UserVM(Seminar1Context context)
        {
            _context = context;
        }

        public List<User> GetAll()
        {
            List<User> users = _context.Users.ToList();
            return users;
        }
    }
}
