using Agile_Project.Controllers;
using Agile_Project.Models;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Console
{
    public class UserStoryMenuUI
    {
        private readonly IProjectController _projectController;
        private readonly IUserStoryController _storyController;

        public UserStoryMenuUI(IProjectController projectController, IUserStoryController storyController)
        {
            _projectController = projectController;
            _storyController = storyController;
        }

        public void Run()
        {
            // Add/Delete are Admin/PO only; state change is also open to Dev.
            bool canManage = PermissionService.CanDo("ManageUserStory");
            bool canChangeState = PermissionService.CanDo("ChangeUserStoryState");

            while (true)
            {
                System.Console.Clear();
                System.Console.WriteLine("=== USER STORY MENU ===");
                System.Console.WriteLine("[1] View user stories by project");
                if (canManage) System.Console.WriteLine("[2] Add user story");
                if (canManage) System.Console.WriteLine("[3] Delete user story");
                if (canChangeState) System.Console.WriteLine("[4] Change user story state");
                System.Console.WriteLine("[0] Back");
                System.Console.Write("Choose: ");

                switch (System.Console.ReadLine())
                {
                    case "1": ViewByProject(); break;
                    case "2": if (canManage) AddUserStory(); else Deny(); break;
                    case "3": if (canManage) DeleteUserStory(); else Deny(); break;
                    case "4": if (canChangeState) ChangeState(); else Deny(); break;
                    case "0": return;
                    default:
                        System.Console.WriteLine("Invalid option.");
                        ConsoleUI.Pause();
                        break;
                }
            }
        }

        private static void Deny()
        {
            System.Console.WriteLine("Permission denied.");
            ConsoleUI.Pause();
        }

        private int PickProject()
        {
            var projects = _projectController.GetAllProjects();
            if (projects.Count == 0)
            {
                System.Console.WriteLine("No projects found.");
                ConsoleUI.Pause();
                return -1;
            }
            System.Console.WriteLine("=== SELECT PROJECT ===");
            foreach (var p in projects)
                System.Console.WriteLine($"[{p.ProjectId}] {p.Name}");
            System.Console.Write("Enter Project ID: ");
            return int.TryParse(System.Console.ReadLine(), out int id) ? id : -1;
        }

        private void ViewByProject()
        {
            System.Console.Clear();
            int projectId = PickProject();
            if (projectId < 0) return;

            var stories = _storyController.GetByProject(projectId);
            System.Console.WriteLine("\n=== USER STORIES ===");
            if (stories.Count == 0)
                System.Console.WriteLine("No user stories found.");
            else
                foreach (var s in stories)
                    System.Console.WriteLine($"[{s.UserStoryId}] {s.Title} | State: {s.State} | Priority: {s.Priority}");
            ConsoleUI.Pause();
        }

        private void AddUserStory()
        {
            System.Console.Clear();
            System.Console.WriteLine("=== ADD USER STORY ===");
            int projectId = PickProject();
            if (projectId < 0) return;

            System.Console.Write("Title: ");
            var title = System.Console.ReadLine() ?? "";
            System.Console.Write("Description: ");
            var desc = System.Console.ReadLine() ?? "";
            System.Console.Write("Priority (0-5): ");
            int.TryParse(System.Console.ReadLine(), out int priority);

            var result = _storyController.AddUserStory(projectId, title, desc, priority);
            System.Console.WriteLine(result ? "User story added!" : "Failed  title cannot be empty.");
            ConsoleUI.Pause();
        }

        private void DeleteUserStory()
        {
            System.Console.Clear();
            int projectId = PickProject();
            if (projectId < 0) return;

            var stories = _storyController.GetByProject(projectId);
            foreach (var s in stories)
                System.Console.WriteLine($"[{s.UserStoryId}] {s.Title} | State: {s.State}");

            System.Console.Write("Enter User Story ID to delete: ");
            if (int.TryParse(System.Console.ReadLine(), out int id))
            {
                _storyController.DeleteUserStory(id);
                System.Console.WriteLine("User story deleted.");
            }
            else
            {
                System.Console.WriteLine("Invalid ID.");
            }
            ConsoleUI.Pause();
        }

        private void ChangeState()
        {
            System.Console.Clear();
            int projectId = PickProject();
            if (projectId < 0) return;

            var stories = _storyController.GetByProject(projectId);
            foreach (var s in stories)
                System.Console.WriteLine($"[{s.UserStoryId}] {s.Title} | State: {s.State}");

            System.Console.Write("Enter User Story ID: ");
            if (!int.TryParse(System.Console.ReadLine(), out int id)) return;

            System.Console.WriteLine("States: [0] ProjectBacklog  [1] InSprint  [2] Done");
            System.Console.Write("Enter new state number: ");
            if (!int.TryParse(System.Console.ReadLine(), out int stateNum)) return;

            var newState = (UserStoryState)stateNum;
            var (success, message) = _storyController.ChangeState(id, newState);
            System.Console.WriteLine(message);
            ConsoleUI.Pause();
        }
    }
}