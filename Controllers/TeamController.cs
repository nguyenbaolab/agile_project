using Agile_Project.Models.Entities;
using Agile_Project.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Agile_Project.Controllers
{
    // Team rules:
    // - one team belongs to one project,
    // - a member must already be on the project,
    // - Admin profiles cannot be team members.
    public class TeamController : ITeamController
    {
        private readonly ITeamRepository _teamRepo;
        private readonly IPersonRepository _personRepo;

        public TeamController(ITeamRepository teamRepo, IPersonRepository personRepo)
        {
            _teamRepo = teamRepo;
            _personRepo = personRepo;
        }

        public TeamController() : this(new TeamRepository(), new PersonRepository()) { }

        // Teams

        public List<Team> GetByProject(int projectId)
            => _teamRepo.GetByProject(projectId);

        public Team? GetById(int teamId)
            => _teamRepo.GetById(teamId);

        public (int teamId, string message) AddTeam(int projectId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (-1, "Team name is required.");

            int newId = _teamRepo.AddAndGetId(new Team
            {
                ProjectId = projectId,
                Name = name.Trim()
            });
            return (newId, "Team added.");
        }

        public (bool success, string message) UpdateTeam(int teamId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Team name is required.");

            var team = _teamRepo.GetById(teamId);
            if (team == null) return (false, "Team not found.");

            team.Name = name.Trim();
            _teamRepo.Update(team);
            return (true, "Team updated.");
        }

        public (bool success, string message) DeleteTeam(int teamId)
        {
            var team = _teamRepo.GetById(teamId);
            if (team == null) return (false, "Team not found.");

            _teamRepo.Delete(teamId);
            return (true, "Team deleted.");
        }

        // Member management

        public List<Person> GetMembers(int teamId)
            => _teamRepo.GetMembers(teamId);

        // Drives the "Add member" dropdown: project-linked, non-Admin, not already in the team.
        public List<Person> GetEligiblePersons(int projectId, int teamId)
        {
            var projectPersons = _personRepo.GetByProject(projectId);
            var current = _teamRepo.GetMembers(teamId).Select(p => p.PersonId).ToHashSet();
            return projectPersons
                .Where(p => !string.Equals(p.ProfileRole, "Admin", StringComparison.OrdinalIgnoreCase))
                .Where(p => !current.Contains(p.PersonId))
                .ToList();
        }

        public (bool success, string message) AddMember(int teamId, int personId)
        {
            var team = _teamRepo.GetById(teamId);
            if (team == null) return (false, "Team not found.");

            var projectPersons = _personRepo.GetByProject(team.ProjectId);
            var target = projectPersons.FirstOrDefault(p => p.PersonId == personId);
            if (target == null)
                return (false, "Person is not linked to this project.");

            if (string.Equals(target.ProfileRole, "Admin", StringComparison.OrdinalIgnoreCase))
                return (false, "Admin users cannot be members of a team.");

            _teamRepo.AddMember(teamId, personId);
            return (true, "Member added.");
        }

        public bool RemoveMember(int teamId, int personId)
        {
            _teamRepo.RemoveMember(teamId, personId);
            return true;
        }
    }
}
