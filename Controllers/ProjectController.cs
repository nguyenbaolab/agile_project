using Agile_Project.Models;
using Agile_Project.Models.Entities;
using Agile_Project.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Controllers
{
    public class ProjectController : IProjectController
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IPersonRepository _personRepo;

        // Constructor used by tests / advanced callers — dependencies are injected
        public ProjectController(IProjectRepository projectRepo, IPersonRepository personRepo)
        {
            _projectRepo = projectRepo;
            _personRepo = personRepo;
        }

        // Convenience constructor — uses the real MySQL-backed repositories
        public ProjectController() : this(new ProjectRepository(), new PersonRepository()) { }

        public List<Project> GetAllProjects()
        {
            return _projectRepo.GetAll();
        }

        public bool AddProject(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            _projectRepo.Add(new Project
            {
                Name = name,
                Description = description
            });
            return true;
        }

        public bool UpdateProject(Project project)
        {
            if (string.IsNullOrWhiteSpace(project.Name)) return false;
            _projectRepo.Update(project);
            return true;
        }

        public void DeleteProject(int projectId)
        {
            _projectRepo.Delete(projectId);
        }

        public List<Person> GetPersonsByProject(int projectId)
        {
            return _personRepo.GetByProject(projectId);
        }

        public bool AddPersonToProject(int projectId, int personId)
        {
            _personRepo.AddToProject(projectId, personId);
            return true;
        }

        public bool RemovePersonFromProject(int projectId, int personId)
        {
            _personRepo.RemoveFromProject(projectId, personId);
            return true;
        }

        public List<Person> GetAllPersons()
        {
            return _personRepo.GetAll();
        }

        public bool AddPerson(string name, string role)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            _personRepo.Add(new Person { Name = name, Role = role });
            return true;
        }

        public (bool success, string message) DeletePerson(int personId)
        {
            bool ok = _personRepo.Delete(personId);
            return ok
                ? (true, "Person deleted.")
                : (false, "System accounts cannot be deleted.");
        }

        // Login
        public (bool success, string message) Login(string username, string password)
        {
            var person = _personRepo.Login(username, password);
            if (person == null) return (false, "Wrong username or password.");

            CurrentSession.PersonId = person.PersonId;
            CurrentSession.Username = person.Username;
            CurrentSession.Role = person.ProfileRole;
            return (true, $"Welcome {person.Name} ({person.ProfileRole})!");
        }
    }
}