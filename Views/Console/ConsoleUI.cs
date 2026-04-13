using Agile_Project.Controllers;

namespace Agile_Project.Views.Console
{
    public class ConsoleUI
    {
        private readonly ProjectController _projectController = new();
        private readonly UserStoryController _userStoryController = new();
        private readonly TaskController _taskController = new();

        public void Run()
        {
            while (true)
            {
                System.Console.Clear();
                System.Console.WriteLine("=== AGILE PROJECT MANAGER ===");
                System.Console.WriteLine("[1] Manage Projects");
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
                        new UserStoryMenuUI(_projectController, _userStoryController).Run();
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