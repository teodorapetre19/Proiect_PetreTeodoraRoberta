using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Seminar_1.Models.VMs;
using System.Buffers.Text;

namespace Seminar_1.Controllers
{
    [Route("[Controller]")]
    public class UserController : Controller
    {
        private readonly Seminar1Context context;

        public UserController(Seminar1Context context)
        {
           this.context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var list = context.Users.Select(p => new UserVM().UserToUserVM(p)).ToList();
            return View(list);
        }

        [HttpGet]
        [Route("New")]
        public IActionResult New()
        {
            var user = new UserVM();
            return View(user);
        }

        [HttpPost]
        [Route("New")]
        public IActionResult Create(UserVM dto)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "There were some errors in your form");
                return View("New", dto);
            }

            if(dto.Password != dto.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Password and ConfirmPaswword do not match");
                return View("New", dto);
            }

            dto.Password = Base64.Base64Encode(dto.ConfirmPassword);
            
            context.Users.Add(UserVM.VMPUserToUser(dto));
            context.SaveChanges();

            return View("Index", context.Users.Select(p => new UserVM().UserToUserVM(p)).ToList());


        }

        [HttpGet]
        [Route("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var user = context.Users.FirstOrDefault(p => p.Id == id);

            if ( user == null)
                return View("Index", context.Users.Select(p => new UserVM().UserToUserVM(p)).ToList());
            else
                return View(new UserVM().UserToUserVM(user));
        }

        [HttpPost]
        [Route("Edit/{id}")]
        public IActionResult Edit(int id, UserVM dto)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "There were some errors in your form");
                return View($"Edit/{id}", dto);
            }

            var user = context.Users.FirstOrDefault(p => p.Id == id);
            if (user == null)
                return View("Index", context.Users.Select(p => new UserVM().UserToUserVM(p)).ToList());


            user.UserName = dto.UserName;
            user.FirstName = dto.FirstName;
            user.SurName = dto.SurName;
            user.Password = dto.Password;

            context.Users.Update(user);
            context.SaveChanges();


            return View("Index", context.Users.Select(p => new UserVM().UserToUserVM(p)).ToList());
        }
        [HttpDelete]
        [Route("Delete/{id}")]
        public JsonResult Delete(int id)
        {
            var user = context.Users.FirstOrDefault(p => p.Id == id);
            if (user == null)
                return Json(new { success = true, message = "Already Deleted" });


            context.Users.Remove(user);
            context.SaveChanges();

            return Json(new { success = true, message = "Delete success" });
        }
    }
}
