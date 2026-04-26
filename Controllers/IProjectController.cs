using System.Collections.Generic;
using Agile_Project.Models.Entities;

namespace Agile_Project.Controllers
{
    public interface IProjectController
    {
        List<Project> GetAllProjects();
        bool AddProject(string name, string description);
        bool UpdateProject(Project project);
        void DeleteProject(int projectId);

        List<Person> GetPersonsByProject(int projectId);
        bool AddPersonToProject(int projectId, int personId);
        bool RemovePersonFromProject(int projectId, int personId);

        List<Person> GetAllPersons();
        bool AddPerson(string name, string role);
        (bool success, string message) DeletePerson(int personId);

        (bool success, string message) Login(string username, string password);
    }
}
