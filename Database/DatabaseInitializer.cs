using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Agile_Project.Models;

namespace Agile_Project.Database
{
    internal class DatabaseInitializer
    {
        public static void Initialize()
        {
            CreateDatabaseIfNotExists();
            CreateTables();
        }

        private static void CreateDatabaseIfNotExists()
        {
            string connStr = "Server=localhost;Port=3306;Uid=root;Pwd=;";
            using var conn = new MySqlConnection(connStr);
            conn.Open();
            var cmd = new MySqlCommand(
                "CREATE DATABASE IF NOT EXISTS agile_db;", conn);
            cmd.ExecuteNonQuery();
        }

        private static void CreateTables()
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();

            var sql = @"
                CREATE TABLE IF NOT EXISTS Projects (
                    ProjectId INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Description TEXT
                );

                CREATE TABLE IF NOT EXISTS Sprints (
                    SprintId INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    StartDate DATE,
                    EndDate DATE,
                    Status VARCHAR(50)
                );

                CREATE TABLE IF NOT EXISTS Teams (
                    TeamId INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Role VARCHAR(100)
                );

                CREATE TABLE IF NOT EXISTS Persons (
                    PersonId INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Role VARCHAR(100)
                );

                CREATE TABLE IF NOT EXISTS UserStories (
                    UserStoryId INT AUTO_INCREMENT PRIMARY KEY,
                    Title VARCHAR(255) NOT NULL,
                    Description TEXT,
                    Priority INT DEFAULT 0,
                    State ENUM('ProjectBacklog','InSprint','Done') DEFAULT 'ProjectBacklog',
                    ProjectId INT NOT NULL,
                    SprintId INT,
                    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId) ON DELETE CASCADE,
                    FOREIGN KEY (SprintId) REFERENCES Sprints(SprintId) ON DELETE SET NULL
                );

                CREATE TABLE IF NOT EXISTS Tasks (
                    TaskId INT AUTO_INCREMENT PRIMARY KEY,
                    Title VARCHAR(255) NOT NULL,
                    Priority INT DEFAULT 0,
                    State ENUM('ToBeDone','InProcess','Done') DEFAULT 'ToBeDone',
                    PlannedTime FLOAT DEFAULT 0,
                    ActualTime FLOAT DEFAULT 0,
                    PlannedStartDate DATE,
                    PlannedEndDate DATE,
                    ActualStartDate DATE,
                    ActualEndDate DATE,
                    Difficulty INT DEFAULT 0,
                    CategoryLabels VARCHAR(500),
                    UserStoryId INT NOT NULL,
                    FOREIGN KEY (UserStoryId) REFERENCES UserStories(UserStoryId) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS ProjectPersons (
                    ProjectId INT,
                    PersonId INT,
                    PRIMARY KEY (ProjectId, PersonId),
                    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId) ON DELETE CASCADE,
                    FOREIGN KEY (PersonId) REFERENCES Persons(PersonId) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS TaskPersons (
                    TaskId INT,
                    PersonId INT,
                    PRIMARY KEY (TaskId, PersonId),
                    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId) ON DELETE CASCADE,
                    FOREIGN KEY (PersonId) REFERENCES Persons(PersonId) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS UserStoryDependencies (
                    UserStoryId INT,
                    DependsOnUserStoryId INT,
                    PRIMARY KEY (UserStoryId, DependsOnUserStoryId),
                    FOREIGN KEY (UserStoryId) REFERENCES UserStories(UserStoryId) ON DELETE CASCADE,
                    FOREIGN KEY (DependsOnUserStoryId) REFERENCES UserStories(UserStoryId) ON DELETE CASCADE
                );
            ";

            foreach (var statement in sql.Split(';'))
            {
                var trimmed = statement.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                using var cmd = new MySqlCommand(trimmed, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
