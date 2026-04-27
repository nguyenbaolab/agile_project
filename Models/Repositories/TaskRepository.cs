using Agile_Project.Models.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agile_Project.Models.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        public List<ProjectTask> GetByUserStory(int userStoryId)
        {
            var list = new List<ProjectTask>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "SELECT * FROM Tasks WHERE UserStoryId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", userStoryId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapFromReader(reader));
            return list;
        }

        public ProjectTask? GetById(int taskId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "SELECT * FROM Tasks WHERE TaskId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", taskId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return MapFromReader(reader);
            return null;
        }

        public void Add(ProjectTask task)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                INSERT INTO Tasks 
                (Title, Priority, State, PlannedTime, ActualTime,
                 PlannedStartDate, PlannedEndDate, ActualStartDate, ActualEndDate,
                 Difficulty, CategoryLabels, UserStoryId)
                VALUES 
                (@Title, @Priority, @State, @PlannedTime, @ActualTime,
                 @PlannedStartDate, @PlannedEndDate, @ActualStartDate, @ActualEndDate,
                 @Difficulty, @CategoryLabels, @UserStoryId)", conn);
            MapToCommand(cmd, task);
            cmd.ExecuteNonQuery();
        }

        public void Update(ProjectTask task)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                UPDATE Tasks SET
                    Title            = @Title,
                    Priority         = @Priority,
                    State            = @State,
                    PlannedTime      = @PlannedTime,
                    ActualTime       = @ActualTime,
                    PlannedStartDate = @PlannedStartDate,
                    PlannedEndDate   = @PlannedEndDate,
                    ActualStartDate  = @ActualStartDate,
                    ActualEndDate    = @ActualEndDate,
                    Difficulty       = @Difficulty,
                    CategoryLabels   = @CategoryLabels
                WHERE TaskId = @TaskId", conn);
            cmd.Parameters.AddWithValue("@Title", task.Title);
            cmd.Parameters.AddWithValue("@Priority", task.Priority);
            cmd.Parameters.AddWithValue("@State", task.State.ToString());
            cmd.Parameters.AddWithValue("@PlannedTime", task.PlannedTime);
            cmd.Parameters.AddWithValue("@ActualTime", task.ActualTime);
            cmd.Parameters.AddWithValue("@PlannedStartDate", (object?)task.PlannedStartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PlannedEndDate", (object?)task.PlannedEndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualStartDate", (object?)task.ActualStartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualEndDate", (object?)task.ActualEndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Difficulty", task.Difficulty);
            cmd.Parameters.AddWithValue("@CategoryLabels", task.CategoryLabels);
            cmd.Parameters.AddWithValue("@TaskId", task.TaskId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int taskId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            // TaskPersons rows are removed by ON DELETE CASCADE
            var cmd = new MySqlCommand(
                "DELETE FROM Tasks WHERE TaskId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", taskId);
            cmd.ExecuteNonQuery();
        }

        public int AddAndGetId(ProjectTask task)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                INSERT INTO Tasks 
                (Title, Priority, State, PlannedTime, ActualTime,
                 PlannedStartDate, PlannedEndDate, ActualStartDate, ActualEndDate,
                 Difficulty, CategoryLabels, UserStoryId)
                VALUES 
                (@Title, @Priority, @State, @PlannedTime, @ActualTime,
                 @PlannedStartDate, @PlannedEndDate, @ActualStartDate, @ActualEndDate,
                 @Difficulty, @CategoryLabels, @UserStoryId);
                SELECT LAST_INSERT_ID();", conn);
            MapToCommand(cmd, task);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void UpdateState(int taskId, TaskState newState)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "UPDATE Tasks SET State=@State WHERE TaskId=@Id", conn);
            cmd.Parameters.AddWithValue("@State", newState.ToString());
            cmd.Parameters.AddWithValue("@Id", taskId);
            cmd.ExecuteNonQuery();
        }

        public void UpdatePriority(int taskId, int priority)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "UPDATE Tasks SET Priority=@Priority WHERE TaskId=@Id", conn);
            cmd.Parameters.AddWithValue("@Priority", priority);
            cmd.Parameters.AddWithValue("@Id", taskId);
            cmd.ExecuteNonQuery();
        }

        public void AssignPerson(int taskId, int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "INSERT IGNORE INTO TaskPersons (TaskId, PersonId) VALUES (@TaskId, @PersonId)", conn);
            cmd.Parameters.AddWithValue("@TaskId", taskId);
            cmd.Parameters.AddWithValue("@PersonId", personId);
            cmd.ExecuteNonQuery();
        }

        public void RemovePerson(int taskId, int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "DELETE FROM TaskPersons WHERE TaskId=@TaskId AND PersonId=@PersonId", conn);
            cmd.Parameters.AddWithValue("@TaskId", taskId);
            cmd.Parameters.AddWithValue("@PersonId", personId);
            cmd.ExecuteNonQuery();
        }

        public void ResetAllToToBeDone(int userStoryId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "UPDATE Tasks SET State='ToBeDone' WHERE UserStoryId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", userStoryId);
            cmd.ExecuteNonQuery();
        }

        public List<Person> GetAssignedPersons(int taskId)
        {
            var list = new List<Person>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT p.* FROM Persons p
                JOIN TaskPersons tp ON p.PersonId = tp.PersonId
                WHERE tp.TaskId = @TaskId", conn);
            cmd.Parameters.AddWithValue("@TaskId", taskId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Person
                {
                    PersonId = Convert.ToInt32(reader["PersonId"]),
                    Name = reader["Name"].ToString()!,
                    Role = reader["Role"].ToString()!
                });
            }
            return list;
        }

        private ProjectTask MapFromReader(MySqlDataReader reader)
        {
            return new ProjectTask
            {
                TaskId = Convert.ToInt32(reader["TaskId"]),
                Title = reader["Title"].ToString()!,
                Priority = Convert.ToInt32(reader["Priority"]),
                State = Enum.Parse<TaskState>(reader["State"].ToString()!),
                PlannedTime = Convert.ToSingle(reader["PlannedTime"]),
                ActualTime = Convert.ToSingle(reader["ActualTime"]),
                PlannedStartDate = reader["PlannedStartDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["PlannedStartDate"]),
                PlannedEndDate = reader["PlannedEndDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["PlannedEndDate"]),
                ActualStartDate = reader["ActualStartDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ActualStartDate"]),
                ActualEndDate = reader["ActualEndDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ActualEndDate"]),
                Difficulty = Convert.ToInt32(reader["Difficulty"]),
                CategoryLabels = reader["CategoryLabels"].ToString()!,
                UserStoryId = Convert.ToInt32(reader["UserStoryId"])
            };
        }

        private void MapToCommand(MySqlCommand cmd, ProjectTask task)
        {
            cmd.Parameters.AddWithValue("@Title", task.Title);
            cmd.Parameters.AddWithValue("@Priority", task.Priority);
            cmd.Parameters.AddWithValue("@State", task.State.ToString());
            cmd.Parameters.AddWithValue("@PlannedTime", task.PlannedTime);
            cmd.Parameters.AddWithValue("@ActualTime", task.ActualTime);
            cmd.Parameters.AddWithValue("@PlannedStartDate", (object?)task.PlannedStartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PlannedEndDate", (object?)task.PlannedEndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualStartDate", (object?)task.ActualStartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualEndDate", (object?)task.ActualEndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Difficulty", task.Difficulty);
            cmd.Parameters.AddWithValue("@CategoryLabels", task.CategoryLabels);
            cmd.Parameters.AddWithValue("@UserStoryId", task.UserStoryId);
        }
    }
}