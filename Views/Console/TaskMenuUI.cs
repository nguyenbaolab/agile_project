using Agile_Project.Controllers;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Console
{
    public class TaskMenuUI
    {
        private readonly IProjectController _projectController;
        private readonly IUserStoryController _storyController;
        private readonly ITaskController _taskController;

        public TaskMenuUI(IProjectController projectController,
            IUserStoryController storyController, ITaskController taskController)
        {
            _projectController = projectController;
            _storyController = storyController;
            _taskController = taskController;
        }

        public void Run()
        {
            while (true)
            {
                System.Console.Clear();
                System.Console.WriteLine("=== TASK MENU ===");
                System.Console.WriteLine("[1] View tasks by user story");
                System.Console.WriteLine("[2] Add task");
                System.Console.WriteLine("[3] Change task state");
                System.Console.WriteLine("[4] Update task priority");
                System.Console.WriteLine("[5] Assign person to task");
                System.Console.WriteLine("[6] Remove person from task");
                System.Console.WriteLine("[7] View task report");
                System.Console.WriteLine("[0] Back");
                System.Console.Write("Choose: ");

                switch (System.Console.ReadLine())
                {
                    case "1": ViewByUserStory(); break;
                    case "2": AddTask(); break;
                    case "3": ChangeState(); break;
                    case "4": UpdatePriority(); break;
                    case "5": AssignPerson(); break;
                    case "6": RemovePerson(); break;
                    case "7": ViewTaskReport(); break;
                    case "0": return;
                    default:
                        System.Console.WriteLine("Invalid option.");
                        ConsoleUI.Pause();
                        break;
                }
            }
        }

        private int PickUserStory()
        {
            var projects = _projectController.GetAllProjects();
            if (projects.Count == 0)
            {
                System.Console.WriteLine("No projects found.");
                ConsoleUI.Pause();
                return -1;
            }
            foreach (var p in projects)
                System.Console.WriteLine($"[{p.ProjectId}] {p.Name}");
            System.Console.Write("Enter Project ID: ");
            if (!int.TryParse(System.Console.ReadLine(), out int projectId)) return -1;

            var stories = _storyController.GetByProject(projectId);
            if (stories.Count == 0)
            {
                System.Console.WriteLine("No user stories found.");
                ConsoleUI.Pause();
                return -1;
            }
            foreach (var s in stories)
                System.Console.WriteLine($"[{s.UserStoryId}] {s.Title} | State: {s.State}");
            System.Console.Write("Enter User Story ID: ");
            return int.TryParse(System.Console.ReadLine(), out int storyId) ? storyId : -1;
        }

        private void ViewByUserStory()
        {
            System.Console.Clear();
            int storyId = PickUserStory();
            if (storyId < 0) return;

            var tasks = _taskController.GetByUserStory(storyId);
            System.Console.WriteLine("\n=== TASKS ===");
            if (tasks.Count == 0)
                System.Console.WriteLine("No tasks found.");
            else
                foreach (var t in tasks)
                    System.Console.WriteLine($"[{t.TaskId}] {t.Title} | State: {t.State} | Priority: {t.Priority}");
            ConsoleUI.Pause();
        }

        private void AddTask()
        {
            System.Console.Clear();
            System.Console.WriteLine("=== ADD TASK ===");
            int storyId = PickUserStory();
            if (storyId < 0) return;

            System.Console.Write("Title: ");
            var title = System.Console.ReadLine() ?? "";
            System.Console.Write("Priority (0-5): ");
            int.TryParse(System.Console.ReadLine(), out int priority);

            var result = _taskController.AddTask(storyId, title, priority);
            System.Console.WriteLine(result ? "Task added!" : "Failed — story may be Done or title empty.");
            ConsoleUI.Pause();
        }

        private void ChangeState()
        {
            System.Console.Clear();
            int storyId = PickUserStory();
            if (storyId < 0) return;

            var tasks = _taskController.GetByUserStory(storyId);
            foreach (var t in tasks)
                System.Console.WriteLine($"[{t.TaskId}] {t.Title} | State: {t.State}");

            System.Console.Write("Enter Task ID: ");
            if (!int.TryParse(System.Console.ReadLine(), out int taskId)) return;

            System.Console.WriteLine("States: [0] ToBeDone  [1] InProcess  [2] Done");
            System.Console.Write("Enter new state number: ");
            if (!int.TryParse(System.Console.ReadLine(), out int stateNum)) return;

            var (success, message) = _taskController.ChangeState(taskId, (TaskState)stateNum);
            System.Console.WriteLine(message);
            ConsoleUI.Pause();
        }

        private void UpdatePriority()
        {
            System.Console.Clear();
            int storyId = PickUserStory();
            if (storyId < 0) return;

            var tasks = _taskController.GetByUserStory(storyId);
            foreach (var t in tasks)
                System.Console.WriteLine($"[{t.TaskId}] {t.Title} | Priority: {t.Priority}");

            System.Console.Write("Enter Task ID: ");
            if (!int.TryParse(System.Console.ReadLine(), out int taskId)) return;
            System.Console.Write("New Priority (0-5): ");
            if (!int.TryParse(System.Console.ReadLine(), out int priority)) return;

            var result = _taskController.UpdatePriority(taskId, priority);
            System.Console.WriteLine(result ? "Priority updated!" : "Failed.");
            ConsoleUI.Pause();
        }

        private void AssignPerson()
        {
            System.Console.Clear();
            System.Console.WriteLine("=== ASSIGN PERSON TO TASK ===");
            int storyId = PickUserStory();
            if (storyId < 0) return;

            var tasks = _taskController.GetByUserStory(storyId);
            foreach (var t in tasks)
                System.Console.WriteLine($"[{t.TaskId}] {t.Title}");
            System.Console.Write("Enter Task ID: ");
            if (!int.TryParse(System.Console.ReadLine(), out int taskId)) return;

            var task = _taskController.GetById(taskId);
            if (task == null) return;

            var story = _storyController.GetById(task.UserStoryId);
            if (story == null) return;

            var persons = _projectController.GetPersonsByProject(story.ProjectId);
            if (persons.Count == 0)
            {
                System.Console.WriteLine("No persons in this project.");
                ConsoleUI.Pause();
                return;
            }
            foreach (var p in persons)
                System.Console.WriteLine($"[{p.PersonId}] {p.Name} — {p.Role}");
            System.Console.Write("Enter Person ID: ");
            if (!int.TryParse(System.Console.ReadLine(), out int personId)) return;

            var (success, message) = _taskController.AssignPerson(taskId, personId);
            System.Console.WriteLine(message);
            ConsoleUI.Pause();
        }

        private void RemovePerson()
        {
            System.Console.Clear();
            System.Console.WriteLine("=== REMOVE PERSON FROM TASK ===");
            int storyId = PickUserStory();
            if (storyId < 0) return;

            var tasks = _taskController.GetByUserStory(storyId);
            foreach (var t in tasks)
                System.Console.WriteLine($"[{t.TaskId}] {t.Title}");
            System.Console.Write("Enter Task ID: ");
            if (!int.TryParse(System.Console.ReadLine(), out int taskId)) return;

            var persons = _taskController.GetAssignedPersons(taskId);
            if (persons.Count == 0)
            {
                System.Console.WriteLine("No persons assigned.");
                ConsoleUI.Pause();
                return;
            }
            foreach (var p in persons)
                System.Console.WriteLine($"[{p.PersonId}] {p.Name}");
            System.Console.Write("Enter Person ID to remove: ");
            if (!int.TryParse(System.Console.ReadLine(), out int personId)) return;

            _taskController.RemovePerson(taskId, personId);
            System.Console.WriteLine("Person removed from task.");
            ConsoleUI.Pause();
        }

        private void ViewTaskReport()
        {
            System.Console.Clear();
            System.Console.Write("Enter Task ID: ");
            if (!int.TryParse(System.Console.ReadLine(), out int taskId)) return;

            var report = _taskController.GetTaskReport(taskId);
            System.Console.WriteLine(report);
            ConsoleUI.Pause();
        }
    }
}