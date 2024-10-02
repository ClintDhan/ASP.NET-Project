using ProjectTask = ASP.NET_Project.Models.Task; // Alias for the Task model
using System;
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
            // Find the user based on email and password
            var user = _context.Users.SingleOrDefault(u => u.Email == email && u.Password == password);

            // Check if the user exists and is active
            if (user != null && user.IsActive)
            {
                // Store user ID in session
                HttpContext.Session.SetInt32("UserId", user.Id);

                // Store username in session
                HttpContext.Session.SetString("UserName", user.UserName);

                // Redirect based on role
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
    var name = HttpContext.Session.GetString("UserName") ?? "";
    ViewBag.Message = name;

    // Get the current user ID from session
    int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

    // Fetch the user including their role and associated projects
    var user = _context.Users
        .Include(u => u.Role)
        .Include(u => u.Projects) // Include associated projects
        .FirstOrDefault(u => u.Id == userId);

    if (user == null)
    {
        return NotFound(); // Handle user not found
    }

    IEnumerable<Project> projects;

    // Determine which projects to show based on the user's role
    if (user.RoleId == 1) // Admin role
    {
        projects = _context.Projects.ToList(); // Fetch all projects
    }
    else if (user.RoleId == 3) // Regular user role
    {
        projects = user.Projects; // Fetch only the user's projects
    }
    else
    {
        projects = Enumerable.Empty<Project>(); // No projects if role is neither
    }

    // Calculate counts for the view
    ViewBag.CompletedCount = projects.Count(p => p.Status == ProjectStatus.Completed);
    ViewBag.InProgressCount = projects.Count(p => p.Status == ProjectStatus.InProgress);
    ViewBag.OverdueCount = projects.Count(p => p.Status == ProjectStatus.Overdue && p.EndDate < DateTime.Now);

    return View("~/Views/Home/Admin/AdminDashboard.cshtml");
}



       public IActionResult AdminProject()
{
    var name = HttpContext.Session.GetString("UserName") ?? "";
    ViewBag.Message = name;

    // Get the current user ID from session
    int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

    // Fetch the user including their role
    var user = _context.Users
        .Include(u => u.Role) // Include Role if necessary
        .FirstOrDefault(u => u.Id == userId);

    if (user == null)
    {
        return NotFound(); // Handle user not found
    }

    // Create the view model
    var model = new AdminProjectsViewModel
    {
        // Get projects based on the user's role
        Projects = user.RoleId == 1 
            ? _context.Projects.ToList() // All projects for admin
            : _context.Projects
                .Where(p => p.ProjectManagerId == userId) // Only user's projects for project managers
                .ToList(),
                
        ProjectManagers = _context.Users
            .Where(u => u.RoleId == 3) // Assuming RoleId 3 is for Project Managers
            .ToList(),
        Users = _context.Users.ToList() // Get all users for the members dropdown
    };

    // Fetch users with RoleId 2
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
                Status = ProjectStatus.InProgress,
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

        [HttpGet]
        public IActionResult DownloadFile(int id)
        {
            // Retrieve the progress entry by its ID
            var progress = _context.Progresses.SingleOrDefault(p => p.Id == id);

            // Check if the progress entry and file exist
            if (progress == null || progress.FileData == null || string.IsNullOrEmpty(progress.FileName))
            {
                return NotFound(); // Return 404 if file not found
            }

            // Return the file for download
            return File(progress.FileData, progress.ContentType, progress.FileName);
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
        public IActionResult EditProject(int projectId, string projectName, int projectManagerId, List<int> members, DateTime startDate, DateTime endDate, string description, ProjectStatus status, bool isActive)
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
                var viewModel = new EditProjectViewModel
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    ProjectManagerId = project.ProjectManagerId,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Description = project.Description,
                    Status = project.Status,
                    IsActive = project.IsActive,
                    Members = _context.Users.Where(u => u.RoleId == 2).ToList() // Get valid users for selection
                };

                return View(viewModel);
            }

            // Update the project details
            project.Name = projectName;
            project.ProjectManagerId = projectManagerId;
            project.StartDate = startDate;
            project.EndDate = endDate;
            project.Description = description;
            project.Status = status; // Directly assign the status
            project.IsActive = isActive;

            // Update project members
            project.Users.Clear();
            var selectedUsers = _context.Users.Where(u => members.Contains(u.Id)).ToList();
            foreach (var user in selectedUsers)
            {
                project.Users.Add(user);
            }

            // Save changes to the database
            _context.SaveChanges();

            return RedirectToAction("AdminProject");
        }

        public IActionResult AdminTask()
        {
            var name = HttpContext.Session.GetString("UserName") ?? "";
            ViewBag.Message = name;
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
                Status = TaskStatus.InProgress
            };

            _context.Tasks.Add(task);
            _context.SaveChanges();

            // Return JSON response if the request was made via AJAX
            return RedirectToAction("AdminTask");
        }

        [HttpPost]
    public IActionResult EditTask(int projectId, int taskId, string taskName, int assignedToId, string description)
    {
        // Fetch the task from the database
        var task = _context.Tasks.Include(t => t.AssignedTo).FirstOrDefault(t => t.Id == taskId);
        
        if (task != null)
        {
            // Update task properties
            task.Name = taskName;
            task.AssignedToId = assignedToId;
            task.Description = description;
            
            _context.SaveChanges();
        }
        
        return RedirectToAction("AdminTask"); // Adjust the redirect as necessary
    }




      public IActionResult AdminUser()
{
    var name = HttpContext.Session.GetString("UserName") ?? "";
    ViewBag.Message = name;

    // Get the current user ID from the session
    int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

    // Fetch the user from the database
    var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userId);

    // Check if the user is found and if they have RoleId 3
    if (user == null || user.RoleId == 3)
    {
        // Redirect to an access denied page
        return RedirectToAction("AccessDenied", "Home"); // Adjust the controller name as needed
    }

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
    var name = HttpContext.Session.GetString("UserName") ?? "";
    ViewBag.Message = name;

    var model = new AdminCompletedProjectsViewModel
    {
        // Get only projects with a status of Completed
        Projects = _context.Projects
            .Where(p => p.Status == ProjectStatus.Completed)
            .ToList()
    };

    return View("~/Views/Home/Admin/AdminCompleted.cshtml", model);
}



       public IActionResult UserDashboard()
{
    var name = HttpContext.Session.GetString("UserName") ?? "";
    ViewBag.Message = name;

    // Get the current user's ID from the session
    int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

    // Fetch the user's projects based on their role
    var ongoingProjects = _context.Projects
        .Where(p => p.Users.Any(u => u.Id == userId) && p.Status == ProjectStatus.InProgress)
        .ToList();

    var completedProjects = _context.Projects
        .Where(p => p.Users.Any(u => u.Id == userId) && p.Status == ProjectStatus.Completed)
        .ToList();

    var overdueProjects = _context.Projects
        .Where(p => p.Users.Any(u => u.Id == userId) && p.Status == ProjectStatus.Overdue)
        .ToList();

    var model = new UserDashboardViewModel
    {
        OngoingProjects = ongoingProjects,
        CompletedProjects = completedProjects,
        OverdueProjects = overdueProjects
    };

    return View("~/Views/Home/User/UserDashboard.cshtml", model);
}



        public IActionResult UserProject()
        {
            var name = HttpContext.Session.GetString("UserName") ?? "";
            ViewBag.Message = name;
            // Get the UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            // Check if the user is not logged in
            if (userId == null)
            {
                // Log the attempt to access without being logged in
                _logger.LogWarning("User attempted to access UserProject without being logged in.");
                return RedirectToAction("Login");
            }

            // Fetch projects associated with the user
            var projects = _context.Projects
                .Where(p => p.Users.Any(u => u.Id == userId)) // Filter projects by users
                .Select(p => new
                {
                    ProjectId = p.Id, // Change to ProjectId
                    ProjectName = p.Name,
                    StartDate = p.StartDate,
                    DueDate = p.EndDate,
                    Status = p.Status
                })
                .ToList();

            // Log the projects for debugging
            _logger.LogInformation("Fetched {Count} projects for User {UserId}", projects.Count, userId);

            // Return the view with the fetched projects
            return View("~/Views/Home/User/UserProject.cshtml", projects);
        }



        public IActionResult UserTask()
        {
            var name = HttpContext.Session.GetString("UserName") ?? "";
            ViewBag.Message = name;
            // Get the current user ID from the session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            // Fetch tasks associated with the user's projects
            var tasks = _context.Tasks
                .Include(t => t.Project) // Include the related project information
                .Where(t => t.Project.Users.Any(u => u.Id == userId)) // Filter tasks by projects the user is a part of
                .Select(t => new
                {
                    ProjectName = t.Project.Name,
                    TaskName = t.Name,
                    Status = t.Status
                })
                .ToList();

            return View("~/Views/Home/User/UserTask.cshtml", tasks);
        }



        public IActionResult ProjectView(int projectId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var name = HttpContext.Session.GetString("UserName") ?? "";
            ViewBag.Message = name;
            // Retrieve project details along with its tasks
            var project = _context.Projects
                .Include(p => p.ProjectManager) // Include the project manager details
                .Include(p => p.Users)          // Include project members
                .Include(p => p.Tasks)          // Include tasks for the project
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
                Description = project.Description,
                Tasks = project.Tasks.ToList() // Ensure this refers to your custom Task type
            };

            return View("~/Views/Home/ProjectView.cshtml", model); // Redirect to the ProjectView with the data
        }

        [HttpPost]
        public IActionResult CreateProgress(Progress model, IFormFile progressFile)
        {
            // Ensure that the UserId is set from the session
            var userId = HttpContext.Session.GetInt32("UserId");

            // Check if UserId is null
            if (userId == null)
            {
                ModelState.AddModelError("UserId", "User must be assigned.");
                return View(model); // Return to the view with an error
            }

            // Create a new Progress object
            var progress = new Progress
            {
                Description = model.Description,
                Date = model.Date,
                TaskId = model.TaskId,
                UserId = userId.Value // Make sure to set the UserId
            };

            // Handle file upload if a file is provided
            if (progressFile != null && progressFile.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    // Copy the file data to memory
                    progressFile.CopyTo(stream);
                    progress.FileData = stream.ToArray();
                    progress.FileName = Path.GetFileName(progressFile.FileName);
                    progress.ContentType = progressFile.ContentType;
                }
            }

            // Add the new Progress entry to the context
            _context.Progresses.Add(progress);

            // Save changes to the database
            _context.SaveChanges();

            // Get the ProjectId from the Task associated with the Progress
            var projectId = _context.Tasks
                .Where(t => t.Id == progress.TaskId)
                .Select(t => t.ProjectId)
                .FirstOrDefault(); // This gets the ProjectId associated with the Task

            // Redirect to the ProjectView after successful operation, passing the ProjectId as a query parameter
            return RedirectToAction("ViewProject", new { projectId });
        }















        private async Task<byte[]> SaveFile(IFormFile progressFile)
        {
            throw new NotImplementedException();
        }

        public IActionResult ViewProject(int projectId)
        {
            var name = HttpContext.Session.GetString("UserName") ?? "";
            ViewBag.Message = name;
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.Find(userId);

            // Retrieve project with its tasks and progress data
            var project = _context.Projects
                .Include(p => p.ProjectManager)
                .Include(p => p.Users)
                .Include(p => p.Tasks)
                .ThenInclude(t => t.Progresses) // Include Progresses for each Task
                .SingleOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            var model = new ProjectViewModel
            {
                ProjectName = project.Name,
                ProjectManager = project.ProjectManager.UserName,
                ProjectMembers = project.Users.Select(u => u.UserName).ToList(),
                StartDate = project.StartDate,
                DueDate = project.EndDate,
                Description = project.Description,
                Tasks = project.Tasks.ToList(),
                Progresses = project.Tasks.SelectMany(t => t.Progresses).ToList() // Collect all progress data
            };

            ViewData["IsAdmin"] = user?.RoleId == 1 || user?.RoleId == 3;

            return View("ProjectView", model);
        }



        public class ProjectViewModel
        {
            public string ProjectName { get; set; }
            public string ProjectManager { get; set; }
            public List<User> ProjectManagers { get; set; } // Add this property

            public List<string> ProjectMembers { get; set; } = new List<string>(); // Initialize to avoid null
            public DateTime StartDate { get; set; }
            public DateTime DueDate { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; } // Add this to the ProjectViewModel
            public List<Progress> Progresses { get; set; } // Include progress data

            public List<ASP.NET_Project.Models.Task> Tasks { get; set; } = new List<ASP.NET_Project.Models.Task>(); // Specify the full namespace
        }

        public class TaskProgressViewModel
        {
            public string TaskName { get; set; }
            public TaskStatus Status { get; set; }
            public List<ProgressViewModel> ProgressList { get; set; }
        }

        public class ProgressViewModel
        {
            public string Description { get; set; }
            public string UserName { get; set; }
            public DateTime Date { get; set; }
            public string FileName { get; set; }
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    internal class UserDashboardViewModel
    {
        public List<Project> OngoingProjects { get; set; }
        public List<Project> CompletedProjects { get; set; }
        public List<Project> OverdueProjects { get; set; }
    }

    internal class AdminCompletedProjectsViewModel
    {
        public List<Project> Projects { get; set; }
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
        public bool IsActive { get; set; }
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