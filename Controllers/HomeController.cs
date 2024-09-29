using ProjectTask = ASP.NET_Project.Models.Task; // Alias for the Task model

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ASP.NET_Project.Models;
using Microsoft.EntityFrameworkCore;
using TaskStatus = ASP.NET_Project.Models.TaskStatus;

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
            var user = _context.Users.SingleOrDefault(u => u.Email == email && u.Password == password);

            if (user != null && user.IsActive)
            {
                // Store user ID in session
                HttpContext.Session.SetInt32("UserId", user.Id);

                if (user.RoleId == 1 || user.RoleId == 3)
                {
                    return RedirectToAction("AdminDashboard");
                }
                else if (user.RoleId == 2)
                {
                    return RedirectToAction("UserDashboard");
                }
            }

            ViewBag.ErrorMessage = "Invalid email or password.";
            return View();
        }

        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Redirect to the login page
            return RedirectToAction("Login");
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
     .Where(u => u.RoleId == 2)
     .ToList();

            ViewBag.Users = users;

            return View("~/Views/Home/Admin/AdminProject.cshtml", model);
        }

[HttpPost]
public IActionResult CreateProject(string projectName, int projectManagerId, List<int> members, DateTime dueDate, string description)
{
    // Retrieve the user ID from session
    var userId = HttpContext.Session.GetInt32("UserId");

    if (userId == null)
    {
        return RedirectToAction("Login");
    }

    // Create a new project
    var project = new Project
    {
        Name = projectName,
        StartDate = DateTime.Now,
        EndDate = dueDate,
        Description = description,
        Status = ProjectStatus.Pending,
        ProjectManagerId = projectManagerId,
        CreatedById = userId.Value // Assign the current user as the creator
    };

    // Check if members list is not null and contains valid IDs
    if (members != null && members.Count > 0)
    {
        foreach (var memberId in members)
        {
            var user = _context.Users.Find(memberId);
            if (user != null)
            {
                project.Users.Add(user); // Add user to the project's Users collection
            }
        }
    }

    _context.Projects.Add(project);
    _context.SaveChanges();

    return RedirectToAction("AdminProject");
}





        [HttpPost]
        public ActionResult DeleteTask(int id)
        {
            // Find the task by id and delete it
            var task = _context.Tasks.Find(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                _context.SaveChanges();
            }

            // Redirect back to the task list after deletion
            return RedirectToAction("AdminTask"); // Adjust this to redirect to the appropriate action
        }
       [HttpGet]
public JsonResult GetProjectMembers(int projectId)
{
    var members = _context.Projects
        .Where(p => p.Id == projectId)
        .SelectMany(p => p.Users.Select(u => new { Id = u.Id, UserName = u.UserName }))
        .ToList();

    return Json(members);
}






    [HttpPost]
public IActionResult EditProject(int projectId, string projectName, int projectManagerId, List<int> members, DateTime startDate, DateTime endDate, string description, ProjectStatus status)
{
    // Retrieve the project from the database
    var project = _context.Projects.Include(p => p.Users).SingleOrDefault(p => p.Id == projectId);

    if (project == null)
    {
        return NotFound();
    }

    // Validate start and end dates
    if (endDate < startDate)
    {
        ModelState.AddModelError("EndDate", "Due date cannot be before the start date.");
    }

    if (startDate > endDate)
    {
        ModelState.AddModelError("StartDate", "Start date cannot be after the due date.");
    }

    // If there are validation errors, return to the view
    if (!ModelState.IsValid)
    {
        // Optionally, reload the project data for the view model
        var viewModel = new EditProjectViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            ProjectManagerId = project.ProjectManagerId,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Description = project.Description,
            Status = project.Status,
            Members = _context.Users.Where(u => u.RoleId == 2).ToList() // Get valid users for selection
        };
        
        return View(viewModel); // Return the view with the existing project data and validation errors
    }

    // Update the project details
    project.Name = projectName;
    project.ProjectManagerId = projectManagerId;
    project.StartDate = startDate;

    // Log and validate endDate
    project.EndDate = endDate; // Set endDate without checking again, as it has been validated
    project.Description = description;
    project.Status = status;

    // Clear existing members and update with new selection
    project.Users.Clear();
    var selectedUsers = _context.Users.Where(u => members.Contains(u.Id)).ToList();
    foreach (var user in selectedUsers)
    {
        project.Users.Add(user);
    }

    // Save changes to the database
    _context.SaveChanges();

    // Redirect back to the project list page
    return RedirectToAction("AdminProject");
}








        public IActionResult AdminTask()
        {
            var model = new AdminTasksViewModel
            {
                Tasks = _context.Tasks.Include(t => t.Project).Include(t => t.AssignedTo).ToList(), // Include project and assigned user
                Projects = _context.Projects.ToList(), // Get all projects
                Users = _context.Users.Where(u => u.RoleId == 2).ToList() // Only get users with RoleId 2
            };

            return View("~/Views/Home/Admin/AdminTask.cshtml", model);
        }


        [HttpPost]
        public IActionResult CreateTask(string taskName, string description, int projectId, int assignedToId)
        {
            if (string.IsNullOrEmpty(taskName) || projectId <= 0 || assignedToId <= 0)
            {
                return BadRequest("Invalid data.");
            }

            var task = new ProjectTask
            {
                Name = taskName,
                Description = description,
                ProjectId = projectId,
                AssignedToId = assignedToId,
                Status = TaskStatus.Pending
            };

            _context.Tasks.Add(task);
            _context.SaveChanges();

            // Return JSON response if the request was made via AJAX
            return RedirectToAction("AdminTask");
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
        public IActionResult ViewProject(int projectId)
{
    // Retrieve project details from the database
    var project = _context.Projects
        .Include(p => p.ProjectManager)   // Include the project manager details
        .Include(p => p.Users)            // Include project members
        .SingleOrDefault(p => p.Id == projectId);

    if (project == null)
    {
        return NotFound(); // Handle case when project is not found
    }

    // Create a view model to pass data to the view
    var model = new ProjectViewModel
    {
        ProjectName = project.Name,
        ProjectManager = project.ProjectManager.UserName,
        ProjectMembers = project.Users.Select(u => u.UserName).ToList(),
        StartDate = project.StartDate,
        DueDate = project.EndDate,
        Description = project.Description
    };

    return View("ProjectView", model); // Redirect to the ProjectView with the data
}
public class ProjectViewModel
{
    public string ProjectName { get; set; }
    public string ProjectManager { get; set; }
    public List<string> ProjectMembers { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Description { get; set; }
}



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    internal class EditProjectViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? ProjectManagerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public ProjectStatus Status { get; set; }
        public List<User> Members { get; set; }
    }

    internal class AdminTasksViewModel
    {
        public List<ASP.NET_Project.Models.Task> Tasks { get; set; } // Fully qualify Task
        public List<Project> Projects { get; set; }
        public List<User> Users { get; set; }
    }


    internal class AdminProjectsViewModel
    {
        public List<Project> Projects { get; set; }
        public List<User> ProjectManagers { get; set; }
        public List<User> Users { get; set; }
    }
}
