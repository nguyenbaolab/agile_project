using System.Collections.Generic;
using Agile_Project.Models.Entities;

namespace Agile_Project.Models.Repositories
{
    public interface IProjectRepository
    {
        List<Project> GetAll();
        Project? GetById(int id);
        void Add(Project project);
        void Update(Project project);
        void Delete(int projectId);
    }
}
