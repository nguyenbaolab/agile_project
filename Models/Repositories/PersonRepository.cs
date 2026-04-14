using Agile_Project.Models.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile_Project.Models.Repositories
{
    public class PersonRepository
    {
        public List<Person> GetAll()
        {
            var list = new List<Person>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM Persons", conn);
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

        public List<Person> GetByProject(int projectId)
        {
            var list = new List<Person>();
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT p.* FROM Persons p
                JOIN ProjectPersons pp ON p.PersonId = pp.PersonId
                WHERE pp.ProjectId = @ProjectId", conn);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
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

        public void Add(Person person)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "INSERT INTO Persons (Name, Role) VALUES (@Name, @Role)", conn);
            cmd.Parameters.AddWithValue("@Name", person.Name);
            cmd.Parameters.AddWithValue("@Role", person.Role);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "DELETE FROM Persons WHERE PersonId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", personId);
            cmd.ExecuteNonQuery();
        }

        public void AddToProject(int projectId, int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "INSERT IGNORE INTO ProjectPersons (ProjectId, PersonId) VALUES (@ProjectId, @PersonId)", conn);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            cmd.Parameters.AddWithValue("@PersonId", personId);
            cmd.ExecuteNonQuery();
        }

        public void RemoveFromTask(int taskId, int personId)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "DELETE FROM TaskPersons WHERE TaskId=@TaskId AND PersonId=@PersonId", conn);
            cmd.Parameters.AddWithValue("@TaskId", taskId);
            cmd.Parameters.AddWithValue("@PersonId", personId);
            cmd.ExecuteNonQuery();
        }

        // --- Login (thêm mới) ---
        public Person? Login(string username, string password)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            var cmd = new MySqlCommand(
                "SELECT * FROM Persons WHERE Username=@U AND Password=@P", conn);
            cmd.Parameters.AddWithValue("@U", username);
            cmd.Parameters.AddWithValue("@P", password);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Person
                {
                    PersonId = Convert.ToInt32(reader["PersonId"]),
                    Name = reader["Name"].ToString()!,
                    Role = reader["Role"].ToString()!,
                    Username = reader["Username"].ToString()!,
                    ProfileRole = reader["ProfileRole"].ToString()!
                };
            }
            return null;
        }
    }
}