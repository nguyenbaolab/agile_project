using Agile_Project.Controllers;
using Agile_Project.Models;

namespace Agile_Project.Views.Console
{
    public class ConsoleUI
    {
        private readonly IProjectController _projectController = new ProjectController();
        private readonly IUserStoryController _userStoryController = new UserStoryController();
        private readonly ITaskController _taskController = new TaskController();

        public void Run()
        {
            // Login truoc khi vao menu //English translation: Login before entering the menu
            if (!new LoginUI(_projectController).Run()) return;

            while (true)
            {
                System.Console.Clear();
                System.Console.WriteLine("=== AGILE PROJECT MANAGER ===");
                System.Console.WriteLine($"Logged in as: {CurrentSession.Username} ({CurrentSession.Role})\n");

                System.Console.WriteLine("[1] Manage Projects");

                // Dev can change story state, so the menu is open to anyone with that right.
                // Add/Edit/Delete inside it stay gated on ManageUserStory.
                if (PermissionService.CanDo("ChangeUserStoryState"))
                    System.Console.WriteLine("[2] Manage User Stories");

                System.Console.WriteLine("[3] Manage Tasks");
                System.Console.WriteLine("[0] Exit");
                System.Console.Write("Choose: ");

                switch (System.Console.ReadLine())
                {
                    case "1":
                        new ProjectMenuUI(_projectController).Run();
                        break;
                    case "2":
                        if (PermissionService.CanDo("ChangeUserStoryState"))
                            new UserStoryMenuUI(_projectController, _userStoryController).Run();
                        else
                        {
                            System.Console.WriteLine("Permission denied.");
                            Pause();
                        }
                        break;
                    case "3":
                        new TaskMenuUI(_projectController, _userStoryController, _taskController).Run();
                        break;
                    case "0":
                        return;
                    default:
                        System.Console.WriteLine("Invalid option.");
                        Pause();
                        break;
                }
            }
        }

        public static void Pause()
        {
            System.Console.WriteLine("\nPress any key to continue...");
            System.Console.ReadKey();
        }
    }
}