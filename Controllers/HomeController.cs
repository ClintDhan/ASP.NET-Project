using ProjectTask = ASP.NET_Project.Models.Task; // Alias for the Task model
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ASP.NET_Project.Models;
using Microsoft.EntityFrameworkCore;
using TaskStatus = ASP.NET_Project.Models.TaskStatus;
using ASP.NET_Project.Services;
namespace ASP.NET_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context; // Use your actual context class
        private readonly ILogger<HomeController> _logger;
        private readonly EmailService _emailService;


        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, EmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }




[HttpPost]
public IActionResult Login(string email, string password)
{
    var user = _context.Users.SingleOrDefault(u => u.Email == email && u.Password == password);

    if (user != null)
    {
        if (!user.IsActive)
        {
            ViewBag.ErrorMessage = "Your account is Inactive. Contact an Admin to change Account Status.";
            return View();
        }

        // Set session variables
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.UserName);
        HttpContext.Session.SetInt32("RoleId", user.RoleId);

        // Generate OTP
        string otp = GenerateOtp();
        _emailService.SendOtpEmail(email, otp);

        // Store OTP and email in session
        HttpContext.Session.SetString("Otp", otp);
        HttpContext.Session.SetString("Email", email);
        TempData["SuccessMessage"] = $"OTP has been sent to {email}.";


        // Redirect to the OtpView
        return RedirectToAction("OtpView");
    }

    ViewBag.ErrorMessage = "Invalid email or password.";
    return View();
}





[HttpPost]
public IActionResult ValidateOtp(string otpInput)
{
    // Retrieve data from session
    string storedOtp = HttpContext.Session.GetString("Otp");
    string email = HttpContext.Session.GetString("Email");

    // Check if the OTP matches
    if (storedOtp != otpInput)
    {
        ViewBag.ErrorMessage = "Invalid OTP. Please try again.";
        return View("OtpView");
    }

    // Find the user based on the stored email
    var user = _context.Users.SingleOrDefault(u => u.Email == email);

    // Check if the user exists
    if (user == null)
    {
        ViewBag.ErrorMessage = "User not found. Please try again.";
        return View("OtpView");
    }

    // Set session variables for the user
    HttpContext.Session.SetInt32("UserId", user.Id);
    HttpContext.Session.SetString("UserName", user.UserName);
    HttpContext.Session.SetInt32("RoleId", user.RoleId);

    // Redirect based on user role
    if (user.RoleId == 1 || user.RoleId == 3)
    {
        return RedirectToAction("AdminDashboard");
    }
    else if (user.RoleId == 2)
    {
        return RedirectToAction("UserDashboard");
    }

    ViewBag.ErrorMessage = "User role is not recognized. Please contact support.";
    return View("OtpView");
}









        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult OtpView()
        {
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
              // Generate OTP
            string otp = GenerateOtp();

            // Log the OTP for debugging purposes
            Console.WriteLine("OTP generated: " + otp);

            // Store user details in session (so they can be accessed after OTP verification)
            HttpContext.Session.SetString("UserName", UserName);
            HttpContext.Session.SetString("Email", Email);
            HttpContext.Session.SetString("Password", Password);
            HttpContext.Session.SetInt32("RoleId", RoleId);
            HttpContext.Session.SetString("Otp", otp);

            // Send OTP to email
            try
            {
                _emailService.SendOtpEmail(Email, otp);
                TempData["SuccessMessage"] = $"OTP has been sent to {Email}.";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "There was an error sending the OTP email. Please try again.";
                return View("Signup");
            }

            // Redirect to OTP verification view
            return RedirectToAction("OtpRegView");

            // var user = new User
            // {
            //     UserName = UserName,
            //     Email = Email,
            //     Password = Password, // Store plain text as per your preference
            //     RoleId = 2,
            //     IsActive = true
            // };

            // _context.Users.Add(user);
            // _context.SaveChanges();

            // return RedirectToAction("Login"); // Redirect to login after successful registration
        }
        [HttpPost]
        public IActionResult ValidateRegistrationOtp(string otpInput)
        {
            // Retrieve stored OTP and user details from session
            string storedOtp = HttpContext.Session.GetString("Otp");
            string storedEmail = HttpContext.Session.GetString("Email");
            string storedUserName = HttpContext.Session.GetString("UserName");
            string storedPassword = HttpContext.Session.GetString("Password");
            int? storedRoleId = HttpContext.Session.GetInt32("RoleId");

            // Check if OTP matches
            if (storedOtp != otpInput)
            {
                ViewBag.ErrorMessage = "Invalid OTP. Please try again.";
                return View("OtpRegView");
            }

            // Check if user details exist in the session
            if (storedEmail == null || storedUserName == null || storedPassword == null || storedRoleId == null)
            {
                ViewBag.ErrorMessage = "Error processing registration. Please try again.";
                return View("OtpRegView");
            }

            // Now create the user in the database
            var user = new User
            {
                UserName = storedUserName,
                Email = storedEmail,
                Password = storedPassword, // Assuming plain text, adjust to hash if needed
                // RoleId = storedRoleId.Value,
                IsActive = true,
                RoleId = 2
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Clear session after successful registration
            HttpContext.Session.Clear();

            // Return success response
    return Json(new { success = true, message = "Successfully created the account!" });
        }

        // Sample method to generate an OTP

        public IActionResult OtpRegView()
        {
            return View(); // This view should have a form to input OTP
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

            IEnumerable<Project> ongoingProjects;
            IEnumerable<Project> completedProjects;
            IEnumerable<Project> overdueProjects;

            // Determine which projects to show based on the user's role
            if (user.RoleId == 1) // Admin role
            {
                // Fetch all projects for admin
                ongoingProjects = _context.Projects.Where(p => p.Status == ProjectStatus.InProgress).ToList();
                completedProjects = _context.Projects.Where(p => p.Status == ProjectStatus.Completed).ToList();
                overdueProjects = _context.Projects.Where(p => p.Status == ProjectStatus.Overdue && p.EndDate < DateTime.Now).ToList();
            }
            else // For RoleId == 2 or 3 or any other role
            {
                ongoingProjects = _context.Projects.Where(p => p.ProjectManagerId == userId && p.Status == ProjectStatus.InProgress).ToList();
                completedProjects = _context.Projects.Where(p => p.ProjectManagerId == userId && p.Status == ProjectStatus.Completed).ToList();
                overdueProjects = _context.Projects.Where(p => p.ProjectManagerId == userId && p.Status == ProjectStatus.Overdue && p.EndDate < DateTime.Now).ToList();
            }

            // Calculate counts for the view
            ViewBag.CompletedCount = completedProjects.Count();
            ViewBag.InProgressCount = ongoingProjects.Count();
            ViewBag.OverdueCount = overdueProjects.Count();

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
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(); // Handle user not found
            }

            // Update project statuses based on overdue conditions for all projects
            var allProjects = _context.Projects.ToList();
            foreach (var project in allProjects)
            {
                UpdateProjectStatusIfOverdue(project.Id); // Call the method to check and update status
            }

            // Get the current date for filtering projects
            var currentDate = DateTime.Now;

            // Filter projects based on the user's role
            var projects = user.RoleId == 1
                ? _context.Projects
                    .Where(p => p.EndDate < currentDate || p.EndDate >= currentDate) // Overdue or ongoing projects
                    .ToList()
                : _context.Projects
                    .Where(p => p.ProjectManagerId == userId && (p.EndDate < currentDate || p.EndDate >= currentDate))
                    .ToList();

            // Create the view model
            var model = new AdminProjectsViewModel
            {
                Projects = projects,
                ProjectManagers = _context.Users
                    .Where(u => u.RoleId == 3)
                    .ToList(),
                Users = _context.Users.ToList()
            };

            // Fetch users with RoleId 2
            var users = _context.Users
                .Where(u => u.RoleId == 2)
                .ToList();

            ViewBag.Users = users;

            return View("~/Views/Home/Admin/AdminProject.cshtml", model);
        }

        public void UpdateProjectStatusIfOverdue(int projectId)
        {
            var project = _context.Projects.SingleOrDefault(p => p.Id == projectId);
            if (project != null && project.Status == ProjectStatus.InProgress && DateTime.Now > project.EndDate)
            {
                project.Status = ProjectStatus.Overdue; // Update the status to Overdue
                project.IsActive = false; // Set the project to InActive
                _context.SaveChanges(); // Save changes to update the database
            }
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
            var project = _context.Projects
                .Include(p => p.Users)
                .Include(p => p.Tasks) // Include tasks to check their statuses
                .SingleOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                return Json(new { success = false, message = "Project not found." });
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

            // Check if trying to set project to Completed when there are InProgress tasks
            if (status == ProjectStatus.Completed)
            {
                // Check if any associated tasks are not completed
                bool hasInProgressTasks = project.Tasks.Any(t => t.Status == TaskStatus.InProgress);
                if (hasInProgressTasks)
                {
                    // Add a model state error
                    ModelState.AddModelError("ProjectStatus", "Cannot set project to Completed while there are tasks still In Progress.");
                }
            }

            // If there are validation errors, return JSON with the errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(" ", errors) });
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

            return Json(new { success = true, message = "Project updated successfully." });
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
            ViewBag.Id = userId;

            // Fetch the user from the database
            var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userId);

            // Check if the user is found and if they have RoleId 3
            if (user == null || user.RoleId == 3)
            {
                // Set an error flag in ViewBag
                ViewBag.Error = true;
                ViewBag.ErrorMessage = "Access Denied! You do not have the necessary permissions.";

                // Pass an empty list to the view to avoid null reference
                return View("~/Views/Home/Admin/AdminDashboard.cshtml");
            }

            // Fetch all users from the database
            var users = _context.Users.Include(u => u.Role).ToList();  // Assuming Role is a navigation property

            // Pass the list of users to the view
            return View("~/Views/Home/Admin/AdminUser.cshtml", users);
        }


        [HttpPost]
        public IActionResult UpdateUser(User updatedUser)
        {
            var user = _context.Users.Find(updatedUser.Id);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Track which fields are being updated
            bool isUserNameUpdated = user.UserName != updatedUser.UserName;
            bool isRoleOrIsActiveUpdated = user.RoleId != updatedUser.RoleId || user.IsActive != updatedUser.IsActive;

            // Check if the user is involved in any in-progress projects
            bool hasInProgressProjects = _context.Projects
                .Any(p => p.Users.Any(u => u.Id == user.Id) && p.Status == ProjectStatus.InProgress);

            // Case 1: If both UserName and Role/IsActive are being updated together, block the update
            if (isUserNameUpdated && isRoleOrIsActiveUpdated)
            {
                return Json(new { success = false, message = "You cannot update both the name and role or active status at the same time." });
            }

            // Case 2: If Role or IsActive is being updated while user has in-progress projects, block the update
            if (isRoleOrIsActiveUpdated && hasInProgressProjects)
            {
                return Json(new { success = false, message = "You cannot update the role or active status while the user is part of in-progress projects." });
            }

            // Apply updates only if no conflicts exist
            if (isUserNameUpdated)
            {
                // Only UserName is updated
                user.UserName = updatedUser.UserName;
            }

            if (isRoleOrIsActiveUpdated)
            {
                // Only Role or IsActive is updated
                user.RoleId = updatedUser.RoleId;
                user.IsActive = updatedUser.IsActive;
            }

            // Save changes only if a valid update was made
            if (isUserNameUpdated || isRoleOrIsActiveUpdated)
            {
                _context.SaveChanges();
                return Json(new { success = true, message = "User updated successfully." });
            }

            return Json(new { success = false, message = "No changes were made." });
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

        [HttpPost]
        public IActionResult UpdateTaskStatus(int taskId, string status)
        {
            var task = _context.Tasks.Find(taskId);
            var currentUserId = HttpContext.Session.GetInt32("UserId"); // Assuming you're using session to store the logged-in user's ID
            var currentUser = _context.Users.Find(currentUserId); // Retrieve the current user

            if (task != null && currentUser != null)
            {
                // Check if the user is assigned to the task or has RoleId 1 or 3
                if (task.AssignedToId == currentUser.Id || currentUser.RoleId == 1 || currentUser.RoleId == 3)
                {
                    if (Enum.TryParse(status, out TaskStatus taskStatus))
                    {
                        task.Status = taskStatus;
                        _context.SaveChanges();
                        return Json(new { success = true });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "You do not have permission to update this task." });
                }
            }
            return Json(new { success = false });
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