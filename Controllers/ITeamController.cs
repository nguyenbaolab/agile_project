using System.Collections.Generic;
using Agile_Project.Models.Entities;

namespace Agile_Project.Controllers
{
    public interface ITeamController
    {
        List<Team> GetByProject(int projectId);
        Team? GetById(int teamId);

        (int teamId, string message) AddTeam(int projectId, string name);
        (bool success, string message) UpdateTeam(int teamId, string name);
        (bool success, string message) DeleteTeam(int teamId);

        List<Person> GetMembers(int teamId);
        List<Person> GetEligiblePersons(int projectId, int teamId);
        (bool success, string message) AddMember(int teamId, int personId);
        bool RemoveMember(int teamId, int personId);
    }
}
