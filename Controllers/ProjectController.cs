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
    public class ProjectController
    {
        private readonly ProjectRepository _projectRepo = new();
        private readonly PersonRepository _personRepo = new();

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

        public bool DeletePerson(int personId)
        {
            _personRepo.Delete(personId);
            return true;
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