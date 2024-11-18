using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ARDSPlatform.Services;

namespace ARDSPlatform.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        public ActionResult GetUserDetails(int UserId)
        {
            var user = _userService.GetUserById(UserId);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
    }
}