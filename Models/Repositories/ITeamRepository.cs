using System.Collections.Generic;
using Agile_Project.Models.Entities;

namespace Agile_Project.Models.Repositories
{
    public interface ITeamRepository
    {
        List<Team> GetByProject(int projectId);
        Team? GetById(int teamId);
        int AddAndGetId(Team team);
        void Update(Team team);
        void Delete(int teamId);

        List<Person> GetMembers(int teamId);
        void AddMember(int teamId, int personId);
        void RemoveMember(int teamId, int personId);
    }
}
