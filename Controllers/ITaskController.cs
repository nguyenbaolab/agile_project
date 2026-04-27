
using System.Collections.Generic;
using Agile_Project.Models.Entities;

namespace Agile_Project.Controllers
{
    public interface ITaskController
    {
        bool AddTask(int userStoryId, string title, int priority);
        (int taskId, string message) AddTaskFull(ProjectTask task, List<int> personIds);
        (bool success, string message) UpdateTask(ProjectTask task);
        (bool success, string message) DeleteTask(int taskId);

        (bool success, string message) AssignPerson(int taskId, int personId);
        bool RemovePerson(int taskId, int personId);
        bool UpdatePriority(int taskId, int priority);

        (bool success, string message) ChangeState(int taskId, TaskState newState);

        string GetTaskReport(int taskId);

        List<ProjectTask> GetByUserStory(int userStoryId);
        ProjectTask? GetById(int taskId);
        List<Person> GetAssignedPersons(int taskId);

        (bool success, string message) AssignTeam(int taskId, int teamId);
        bool RemoveTeam(int taskId, int teamId);
        List<Team> GetAssignedTeams(int taskId);
        List<Team> GetProjectTeams(int projectId);
    }
}
