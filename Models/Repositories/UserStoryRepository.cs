using Agile_Project.Models.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models.Repositories
{
    public class UserStoryRepository
    {
        public List<UserStory> GetByProject(int projectId)
        {
            var list = new List<UserStory>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "SELECT * FROM UserStories WHERE ProjectId=@ProjectId", conn);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapFromReader(reader));
            }
            return list;
        }

        public UserStory? GetById(int id)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "SELECT * FROM UserStories WHERE UserStoryId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return MapFromReader(reader);
            return null;
        }

        public void Add(UserStory story)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                INSERT INTO UserStories (Title, Description, Priority, State, ProjectId, SprintId)
                VALUES (@Title, @Description, @Priority, @State, @ProjectId, @SprintId)", conn);
            cmd.Parameters.AddWithValue("@Title", story.Title);
            cmd.Parameters.AddWithValue("@Description", story.Description);
            cmd.Parameters.AddWithValue("@Priority", story.Priority);
            cmd.Parameters.AddWithValue("@State", story.State.ToString());
            cmd.Parameters.AddWithValue("@ProjectId", story.ProjectId);
            cmd.Parameters.AddWithValue("@SprintId", (object?)story.SprintId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void UpdateState(int storyId, UserStoryState newState)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "UPDATE UserStories SET State=@State WHERE UserStoryId=@Id", conn);
            cmd.Parameters.AddWithValue("@State", newState.ToString());
            cmd.Parameters.AddWithValue("@Id", storyId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int storyId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            // CASCADE sẽ tự xóa Tasks và TaskPersons liên quan
            var cmd = new MySqlCommand(
                "DELETE FROM UserStories WHERE UserStoryId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", storyId);
            cmd.ExecuteNonQuery();
        }

        public List<int> GetDependencies(int storyId)
        {
            var list = new List<int>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "SELECT DependsOnUserStoryId FROM UserStoryDependencies WHERE UserStoryId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", storyId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Convert.ToInt32(reader["DependsOnUserStoryId"]));
            return list;
        }

        private UserStory MapFromReader(MySqlDataReader reader)
        {
            return new UserStory
            {
                UserStoryId = Convert.ToInt32(reader["UserStoryId"]),
                Title = reader["Title"].ToString()!,
                Description = reader["Description"].ToString()!,
                Priority = Convert.ToInt32(reader["Priority"]),
                State = Enum.Parse<UserStoryState>(reader["State"].ToString()!),
                ProjectId = Convert.ToInt32(reader["ProjectId"]),
                SprintId = reader["SprintId"] == DBNull.Value ? null : Convert.ToInt32(reader["SprintId"])
            };
        }
    }
}
