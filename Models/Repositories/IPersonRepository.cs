using System.Collections.Generic;
using Agile_Project.Models.Entities;

namespace Agile_Project.Models.Repositories
{
    public interface IPersonRepository
    {
        List<Person> GetAll();
        List<Person> GetByProject(int projectId);
        void Add(Person person);
        bool Delete(int personId);
        void AddToProject(int projectId, int personId);
        void RemoveFromProject(int projectId, int personId);
        void RemoveFromTask(int taskId, int personId);
        Person? Login(string username, string password);
    }
}
