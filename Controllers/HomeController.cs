using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ASP.NET_Project.Models;
using Microsoft.EntityFrameworkCore;
namespace ASP.NET_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context; // Use your actual context class
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
public IActionResult Login(string email, string password)
{
    // Fetch the user from the database based on the email and password
    var user = _context.Users.SingleOrDefault(u => u.Email == email && u.Password == password);

    if (user != null)
    {
        // Check RoleId and redirect accordingly
        if (user.RoleId == 1 || user.RoleId == 3)
        {
            return RedirectToAction("AdminDashboard");
        }
        else if (user.RoleId == 2)
        {
            return RedirectToAction("UserDashboard");
        }
    }

    // If login failed (user is null), return to Login view with an error message
    ViewBag.ErrorMessage = "Invalid email or password.";
    return View();
}


        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string UserName, string Email, string Password, string ConfirmPassword, int RoleId)
        {
            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View("Signup"); // Return to the signup view
            }

            if (_context.Users.Any(u => u.UserName == UserName || u.Email == Email))
            {
                ModelState.AddModelError("", "Username or email already exists.");
                return View("Signup"); // Return to the signup view
            }

            var user = new User
            {
                UserName = UserName,
                Email = Email,
                Password = Password, // Store plain text as per your preference
                RoleId = RoleId,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login"); // Redirect to login after successful registration
        }

        public IActionResult AdminDashboard()
        {
            return View("~/Views/Home/Admin/AdminDashboard.cshtml");
        }

        public IActionResult AdminProject()
        {
            return View("~/Views/Home/Admin/AdminProject.cshtml");
        }

        public IActionResult AdminTask()
        {
            return View("~/Views/Home/Admin/AdminTask.cshtml");
        }

      public IActionResult AdminUser()
{
    // Fetch all users from the database
    var users = _context.Users.Include(u => u.Role).ToList();  // Assuming Role is a navigation property

    // Pass the list of users to the view
    return View("~/Views/Home/Admin/AdminUser.cshtml", users);
}




        public IActionResult AdminCompleted()
        {
            return View("~/Views/Home/Admin/AdminCompleted.cshtml");
        }

        public IActionResult UserDashboard()
        {
            return View("~/Views/Home/User/UserDashboard.cshtml");
        }

        public IActionResult ProjectView()
        {
            return View("~/Views/Home/ProjectView.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
