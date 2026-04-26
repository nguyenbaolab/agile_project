using System.Collections.Generic;
using Agile_Project.Models.Entities;

namespace Agile_Project.Models.Repositories
{
    public interface IUserStoryRepository
    {
        List<UserStory> GetByProject(int projectId);
        UserStory? GetById(int id);
        void Add(UserStory story);
        void Update(UserStory story);
        void UpdateState(int storyId, UserStoryState newState);
        void Delete(int storyId);
        List<int> GetDependencies(int storyId);
    }
}
