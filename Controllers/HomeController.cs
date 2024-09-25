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
                // Check if the user is active
                if (!user.IsActive)
                {
                    ViewBag.ErrorMessage = "Your account is inactive. Please contact support.";
                    return View();
                }

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

            // If login failed (user is null or inactive), return to Login view with an error message
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
            var model = new AdminProjectsViewModel
            {
                Projects = _context.Projects.ToList(), // Get all projects from the database
                ProjectManagers = _context.Users
                    .Where(u => u.RoleId == 3) // Assuming RoleId 3 is for Project Managers
                    .ToList(),
                Users = _context.Users.ToList() // Get all users for the members dropdown
            };

            var users = _context.Users
       .Where(u => u.RoleId == 2 && !_context.Tasks.Any(t => t.AssignedToId == u.Id))
       .ToList();

            ViewBag.Users = users;

            return View("~/Views/Home/Admin/AdminProject.cshtml", model);
        }


      [HttpPost]
public IActionResult CreateProject(string projectName, int projectManagerId, List<int> userIds, DateTime dueDate, string description)
{
    // Create a new project
    var project = new Project
    {
        Name = projectName,
        StartDate = DateTime.Now,
        EndDate = dueDate,
        Description = description,
        Status = ProjectStatus.Pending,
        ProjectManagerId = projectManagerId // Assign the project manager
    };

    // Add the project to the database
    _context.Projects.Add(project);
    _context.SaveChanges(); // Save the project first to generate the project ID

    // Assign users to the project
    var usersToAdd = _context.Users.Where(u => userIds.Contains(u.Id)).ToList();

    // Add users to the project's Users collection one by one
    foreach (var user in usersToAdd)
    {
        project.Users.Add(user);
    }

    // Update the database with the project and its users
    _context.SaveChanges();

    return RedirectToAction("AdminProjects");
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
        [HttpPost]
        public IActionResult UpdateUser(User user)
        {
            // Fetch the user from the database based on the ID
            var existingUser = _context.Users.Find(user.Id);

            if (existingUser != null)
            {
                // Update the fields
                existingUser.UserName = user.UserName;
                existingUser.RoleId = user.RoleId;
                existingUser.IsActive = user.IsActive;

                // Save changes to the database
                _context.SaveChanges();
            }

            // Redirect to the user management page
            return RedirectToAction("AdminUser");
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

    internal class AdminProjectsViewModel
    {
        public List<Project> Projects { get; set; }
        public List<User> ProjectManagers { get; set; }
        public List<User> Users { get; set; }
    }
}
