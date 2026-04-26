using Agile_Project.Models.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        public List<Project> GetAll()
        {
            var projects = new List<Project>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM Projects", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                projects.Add(new Project
                {
                    ProjectId = Convert.ToInt32(reader["ProjectId"]),
                    Name = reader["Name"].ToString()!,
                    Description = reader["Description"].ToString()!
                });
            }
            return projects;
        }

        public Project? GetById(int id)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "SELECT * FROM Projects WHERE ProjectId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Project
                {
                    ProjectId = Convert.ToInt32(reader["ProjectId"]),
                    Name = reader["Name"].ToString()!,
                    Description = reader["Description"].ToString()!
                };
            }
            return null;
        }

        public void Add(Project project)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "INSERT INTO Projects (Name, Description) VALUES (@Name, @Description)", conn);
            cmd.Parameters.AddWithValue("@Name", project.Name);
            cmd.Parameters.AddWithValue("@Description", project.Description);
            cmd.ExecuteNonQuery();
        }

        public void Update(Project project)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "UPDATE Projects SET Name=@Name, Description=@Description WHERE ProjectId=@Id", conn);
            cmd.Parameters.AddWithValue("@Name", project.Name);
            cmd.Parameters.AddWithValue("@Description", project.Description);
            cmd.Parameters.AddWithValue("@Id", project.ProjectId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int projectId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "DELETE FROM Projects WHERE ProjectId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", projectId);
            cmd.ExecuteNonQuery();
        }
    }
}
