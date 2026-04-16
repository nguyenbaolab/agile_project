using Agile_Project.Models.Entities;
using Agile_Project.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Agile_Project.Controllers
{
    public class TaskController
    {
        private readonly TaskRepository _taskRepo = new();
        private readonly UserStoryRepository _storyRepo = new();
        private readonly PersonRepository _personRepo = new();

        public bool AddTask(int userStoryId, string title, int priority)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;

            var story = _storyRepo.GetById(userStoryId);
            if (story == null) return false;
            if (story.State == UserStoryState.Done) return false;

            _taskRepo.Add(new ProjectTask
            {
                UserStoryId = userStoryId,
                Title = title,
                Priority = priority,
                State = TaskState.ToBeDone
            });
            return true;
        }

        public (int taskId, string message) AddTaskFull(ProjectTask task, List<int> personIds)
        {
            if (string.IsNullOrWhiteSpace(task.Title))
                return (-1, "Title is required.");

            var story = _storyRepo.GetById(task.UserStoryId);
            if (story == null) return (-1, "User story not found.");
            if (story.State == UserStoryState.Done) return (-1, "Story is already done.");

            int newId = _taskRepo.AddAndGetId(task);

            foreach (var pid in personIds)
            {
                var projectPersons = _personRepo.GetByProject(story.ProjectId);
                if (projectPersons.Any(p => p.PersonId == pid))
                    _taskRepo.AssignPerson(newId, pid);
            }

            return (newId, "Task added successfully.");
        }

        public (bool success, string message) UpdateTask(ProjectTask task)
        {
            var existing = _taskRepo.GetById(task.TaskId);
            if (existing == null) return (false, "Task not found.");

            // If state changed, go through the ChangeState validation rules
            if (task.State != existing.State)
            {
                var (ok, msg) = ChangeState(task.TaskId, task.State);
                if (!ok) return (false, msg);
            }

            // Persist all other fields
            _taskRepo.Update(task);
            return (true, "Task updated.");
        }

        public (bool success, string message) AssignPerson(int taskId, int personId)
        {
            var task = _taskRepo.GetById(taskId);
            if (task == null) return (false, "Task not found.");

            var story = _storyRepo.GetById(task.UserStoryId);
            if (story == null) return (false, "User story not found.");

            var projectPersons = _personRepo.GetByProject(story.ProjectId);
            if (!projectPersons.Any(p => p.PersonId == personId))
                return (false, "Person is not linked to this project.");

            _taskRepo.AssignPerson(taskId, personId);
            return (true, "Person assigned successfully.");
        }

        public bool RemovePerson(int taskId, int personId)
        {
            _taskRepo.RemovePerson(taskId, personId);
            return true;
        }

        public bool UpdatePriority(int taskId, int priority)
        {
            if (priority < 0) return false;
            _taskRepo.UpdatePriority(taskId, priority);
            return true;
        }

        public (bool success, string message) ChangeState(int taskId, TaskState newState)
        {
            var task = _taskRepo.GetById(taskId);
            if (task == null) return (false, "Task not found.");

            var story = _storyRepo.GetById(task.UserStoryId);
            if (story == null) return (false, "User story not found.");

            if (story.State != UserStoryState.InSprint)
                return (false, "User story must be in sprint.");

            int current = (int)task.State;
            int target = (int)newState;
            if (Math.Abs(current - target) != 1)
                return (false, "Invalid state transition.");

            if (newState == TaskState.InProcess)
            {
                if (!CanSetInProcess(story))
                    return (false, "All tasks of dependent stories must be done first.");
            }

            _taskRepo.UpdateState(taskId, newState);
            return (true, "State updated successfully.");
        }

        public string GetTaskReport(int taskId)
        {
            var task = _taskRepo.GetById(taskId);
            if (task == null) return "Task not found.";

            var story = _storyRepo.GetById(task.UserStoryId);
            var persons = _taskRepo.GetAssignedPersons(taskId);
            var personNames = string.Join(", ", persons.Select(p => p.Name));

            return $"""
                === TASK REPORT ===
                Title:          {task.Title}
                State:          {task.State}
                Priority:       {task.Priority}
                Difficulty:     {task.Difficulty}
                Planned Time:   {task.PlannedTime}h
                Actual Time:    {task.ActualTime}h
                Planned Start:  {task.PlannedStartDate?.ToString("yyyy-MM-dd") ?? "-"}
                Planned End:    {task.PlannedEndDate?.ToString("yyyy-MM-dd") ?? "-"}
                Actual Start:   {task.ActualStartDate?.ToString("yyyy-MM-dd") ?? "-"}
                Actual End:     {task.ActualEndDate?.ToString("yyyy-MM-dd") ?? "-"}
                Labels:         {task.CategoryLabels}
                Assigned to:    {(string.IsNullOrEmpty(personNames) ? "Nobody" : personNames)}
                User Story:     {story?.Title ?? "-"}
                ==================
                """;
        }

        public List<ProjectTask> GetByUserStory(int userStoryId)
            => _taskRepo.GetByUserStory(userStoryId);

        public ProjectTask? GetById(int taskId)
            => _taskRepo.GetById(taskId);

        public List<Person> GetAssignedPersons(int taskId)
            => _taskRepo.GetAssignedPersons(taskId);

        private bool CanSetInProcess(UserStory story)
        {
            var dependencies = _storyRepo.GetDependencies(story.UserStoryId);
            foreach (var depId in dependencies)
            {
                var depTasks = _taskRepo.GetByUserStory(depId);
                if (depTasks.Any(t => t.State != TaskState.Done))
                    return false;
            }
            return true;
        }
    }
}