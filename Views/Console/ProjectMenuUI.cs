using Agile_Project.Controllers;

namespace Agile_Project.Views.Console
{
    public class ProjectMenuUI
    {
        private readonly ProjectController _controller;

        public ProjectMenuUI(ProjectController controller)
        {
            _controller = controller;
        }

        public void Run()
        {
            while (true)
            {
                System.Console.Clear();
                System.Console.WriteLine("=== PROJECT MENU ===");
                System.Console.WriteLine("[1] View all projects");
                System.Console.WriteLine("[2] Add project");
                System.Console.WriteLine("[3] Delete project");
                System.Console.WriteLine("[4] View persons in project");
                System.Console.WriteLine("[5] Add person");
                System.Console.WriteLine("[6] Link person to project");
                System.Console.WriteLine("[0] Back");
                System.Console.Write("Choose: ");

                switch (System.Console.ReadLine())
                {
                    case "1": ViewAll(); break;
                    case "2": AddProject(); break;
                    case "3": DeleteProject(); break;
                    case "4": ViewPersons(); break;
                    case "5": AddPerson(); break;
                    case "6": LinkPerson(); break;
                    case "0": return;
                    default:
                        System.Console.WriteLine("Invalid option.");
                        ConsoleUI.Pause();
                        break;
                }
            }
        }

        private void ViewAll()
        {
            System.Console.Clear();
            System.Console.WriteLine("=== ALL PROJECTS ===");
            var projects = _controller.GetAllProjects();
            if (projects.Count == 0)
            {
                System.Console.WriteLine("No projects found.");
            }
            else
            {
                foreach (var p in projects)
                    System.Console.WriteLine($"[{p.ProjectId}] {p.Name} — {p.Description}");
            }
            ConsoleUI.Pause();
        }

        private void AddProject()
        {
            System.Console.Clear();
            System.Console.WriteLine("=== ADD PROJECT ===");
            System.Console.Write("Name: ");
            var name = System.Console.ReadLine() ?? "";
            System.Console.Write("Description: ");
            var desc = System.Console.ReadLine() ?? "";

            var result = _controller.AddProject(name, desc);
            System.Console.WriteLine(result ? "Project added!" : "Failed — name cannot be empty.");
            ConsoleUI.Pause();
        }

        private void DeleteProject()
        {
            System.Console.Clear();
            ViewAll();
            System.Console.Write("Enter Project ID to delete: ");
            if (int.TryParse(System.Console.ReadLine(), out int id))
            {
                _controller.DeleteProject(id);
                System.Console.WriteLine("Project deleted.");
            }
            else
            {
                System.Console.WriteLine("Invalid ID.");
            }
            ConsoleUI.Pause();
        }

        private void ViewPersons()
        {
            System.Console.Clear();
            ViewAll();
            System.Console.Write("Enter Project ID: ");
            if (int.TryParse(System.Console.ReadLine(), out int id))
            {
                var persons = _controller.GetPersonsByProject(id);
                System.Console.WriteLine("\n=== PERSONS IN PROJECT ===");
                if (persons.Count == 0)
                    System.Console.WriteLine("No persons linked.");
                else
                    foreach (var p in persons)
                        System.Console.WriteLine($"[{p.PersonId}] {p.Name} — {p.Role}");
            }
            ConsoleUI.Pause();
        }

        private void AddPerson()
        {
            System.Console.Clear();
            System.Console.WriteLine("=== ADD PERSON ===");
            System.Console.Write("Name: ");
            var name = System.Console.ReadLine() ?? "";
            System.Console.Write("Role: ");
            var role = System.Console.ReadLine() ?? "";

            var result = _controller.AddPerson(name, role);
            System.Console.WriteLine(result ? "Person added!" : "Failed — name cannot be empty.");
            ConsoleUI.Pause();
        }

        private void LinkPerson()
        {
            System.Console.Clear();
            System.Console.WriteLine("=== LINK PERSON TO PROJECT ===");

            var projects = _controller.GetAllProjects();
            foreach (var p in projects)
                System.Console.WriteLine($"[{p.ProjectId}] {p.Name}");
            System.Console.Write("Enter Project ID: ");
            if (!int.TryParse(System.Console.ReadLine(), out int projectId)) return;

            var persons = _controller.GetAllPersons();
            foreach (var p in persons)
                System.Console.WriteLine($"[{p.PersonId}] {p.Name} — {p.Role}");
            System.Console.Write("Enter Person ID: ");
            if (!int.TryParse(System.Console.ReadLine(), out int personId)) return;

            _controller.AddPersonToProject(projectId, personId);
            System.Console.WriteLine("Person linked to project!");
            ConsoleUI.Pause();
        }
    }
}