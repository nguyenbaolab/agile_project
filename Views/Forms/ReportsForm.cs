using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Agile_Project.Controllers;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Forms
{
    public class ReportsForm : Form
    {
        private readonly Project _project;
        private readonly UserStory? _focusStory;
        private readonly IUserStoryController _storyCtrl;
        private readonly ITaskController _taskCtrl;
        private readonly IProjectController _projectCtrl;

        public ReportsForm(Project project, UserStory? focusStory,
            IUserStoryController storyCtrl, ITaskController taskCtrl, IProjectController projectCtrl)
        {
            _project = project;
            _focusStory = focusStory;
            _storyCtrl = storyCtrl;
            _taskCtrl = taskCtrl;
            _projectCtrl = projectCtrl;

            Text = $"Reports — {project.Name}";
            Size = new Size(720, 560);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            BuildUI();
        }

        private void BuildUI()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill };

            tabs.TabPages.Add(BuildProjectReportTab());
            tabs.TabPages.Add(BuildSprintReportTab());
            tabs.TabPages.Add(BuildUserStoryReportTab());
            tabs.TabPages.Add(BuildPersonReportTab());
            tabs.TabPages.Add(BuildBurndownTab()); // NEW

            Controls.Add(tabs);

            if (_focusStory != null)
                tabs.SelectedIndex = 2;
        }

        // Project Report

        private TabPage BuildProjectReportTab()
        {
            var tp = new TabPage("Project");
            var txt = MakeReportTextBox();
            tp.Controls.Add(txt);

            var stories = _storyCtrl.GetByProject(_project.ProjectId);
            int total = stories.Count;
            int done = stories.Count(s => s.State == UserStoryState.Done);
            double rate = total == 0 ? 0 : Math.Round(done * 100.0 / total, 1);

            var sb = new StringBuilder();
            sb.AppendLine($"PROJECT REPORT: {_project.Name}");
            sb.AppendLine($"Description: {_project.Description}");
            sb.AppendLine(new string('─', 50));
            sb.AppendLine($"Total user stories : {total}");
            sb.AppendLine($"Done               : {done}");
            sb.AppendLine($"In Sprint          : {stories.Count(s => s.State == UserStoryState.InSprint)}");
            sb.AppendLine($"Backlog            : {stories.Count(s => s.State == UserStoryState.ProjectBacklog)}");
            sb.AppendLine($"Completion rate    : {rate}%");
            sb.AppendLine();
            sb.AppendLine("User Stories:");
            sb.AppendLine(new string('─', 50));
            foreach (var s in stories)
            {
                var tasks = _taskCtrl.GetByUserStory(s.UserStoryId);
                int tDone = tasks.Count(t => t.State == TaskState.Done);
                sb.AppendLine($"  [{StateLabel(s.State)}]  {s.Title}  (Priority: {s.Priority}, Tasks: {tDone}/{tasks.Count})");
            }

            txt.Text = sb.ToString();
            return tp;
        }

        // Sprint Report

        private TabPage BuildSprintReportTab()
        {
            var tp = new TabPage("Sprint");
            var txt = MakeReportTextBox();
            tp.Controls.Add(txt);

            var stories = _storyCtrl.GetByProject(_project.ProjectId)
                .Where(s => s.State == UserStoryState.InSprint).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"SPRINT REPORT — {_project.Name}");
            sb.AppendLine(new string('─', 50));

            int totalTasks = 0, doneTasks = 0, overdueTasks = 0;
            float totalPlanned = 0, totalActual = 0;

            foreach (var s in stories)
            {
                var tasks = _taskCtrl.GetByUserStory(s.UserStoryId);
                totalTasks += tasks.Count;
                doneTasks += tasks.Count(t => t.State == TaskState.Done);
                overdueTasks += tasks.Count(t =>
                    t.PlannedEndDate.HasValue && t.PlannedEndDate.Value.Date <= DateTime.Today);
                totalPlanned += tasks.Sum(t => t.PlannedTime);
                totalActual += tasks.Sum(t => t.ActualTime);
            }

            double realRate = totalTasks == 0 ? 0 : Math.Round(doneTasks * 100.0 / totalTasks, 1);
            double plannedRate = totalTasks == 0 ? 0 : Math.Round(overdueTasks * 100.0 / totalTasks, 1);

            sb.AppendLine($"Stories in sprint   : {stories.Count}");
            sb.AppendLine($"Total tasks         : {totalTasks}");
            sb.AppendLine($"Tasks done          : {doneTasks}");
            sb.AppendLine($"Real completion     : {realRate}%");
            sb.AppendLine($"Planned completion  : {plannedRate}%");
            sb.AppendLine($"Total planned time  : {totalPlanned:F1} h");
            sb.AppendLine($"Total actual time   : {totalActual:F1} h");
            sb.AppendLine();
            sb.AppendLine("Stories:");
            sb.AppendLine(new string('─', 50));

            foreach (var s in stories)
            {
                var tasks = _taskCtrl.GetByUserStory(s.UserStoryId);
                int td = tasks.Count(t => t.State == TaskState.Done);
                sb.AppendLine($"  {s.Title}  ({td}/{tasks.Count} done)");
                foreach (var t in tasks)
                    sb.AppendLine($"    [{TaskStateLabel(t.State)}] {t.Title}  ({t.PlannedTime:F1}h planned / {t.ActualTime:F1}h actual)");
            }

            txt.Text = sb.ToString();
            return tp;
        }

        // User Story Report

        private TabPage BuildUserStoryReportTab()
        {
            var tp = new TabPage("User Story");

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var cmbPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(250, 250, 249),
                Padding = new Padding(6, 5, 6, 5),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            var cmbStories = new ComboBox
            {
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10f)
            };
            var allStories = _storyCtrl.GetByProject(_project.ProjectId);
            cmbStories.DataSource = allStories;
            cmbStories.DisplayMember = "Title";
            if (_focusStory != null)
            {
                var match = allStories.FirstOrDefault(s => s.UserStoryId == _focusStory.UserStoryId);
                if (match != null) cmbStories.SelectedItem = match;
            }
            cmbPanel.Controls.Add(cmbStories);
            layout.Controls.Add(cmbPanel, 0, 0);

            var txt = MakeReportTextBox();
            layout.Controls.Add(txt, 0, 1);

            void RefreshStoryReport()
            {
                if (cmbStories.SelectedItem is not UserStory story) return;
                var tasks = _taskCtrl.GetByUserStory(story.UserStoryId);
                int total = tasks.Count;
                int done = tasks.Count(t => t.State == TaskState.Done);
                int overdue = tasks.Count(t => t.PlannedEndDate.HasValue && t.PlannedEndDate.Value.Date < DateTime.Today);
                float planned = tasks.Sum(t => t.PlannedTime);
                float actual = tasks.Sum(t => t.ActualTime);
                double realRate = total == 0 ? 0 : Math.Round(done * 100.0 / total, 1);
                double plannedRate = total == 0 ? 0 : Math.Round(overdue * 100.0 / total, 1);

                var sb = new StringBuilder();
                sb.AppendLine($"USER STORY: {story.Title}");
                sb.AppendLine($"State       : {StateLabel(story.State)}");
                sb.AppendLine($"Priority    : {story.Priority}");
                sb.AppendLine($"Description : {story.Description}");
                sb.AppendLine(new string('─', 50));
                sb.AppendLine($"Total tasks         : {total}");
                sb.AppendLine($"Done                : {done}");
                sb.AppendLine($"Real completion     : {realRate}%");
                sb.AppendLine($"Planned completion  : {plannedRate}%");
                sb.AppendLine($"Total planned time  : {planned:F1} h");
                sb.AppendLine($"Total actual time   : {actual:F1} h");
                sb.AppendLine();
                sb.AppendLine("Tasks:");
                sb.AppendLine(new string('─', 50));
                foreach (var t in tasks)
                    sb.AppendLine($"  [{TaskStateLabel(t.State)}] {t.Title}  (Pri:{t.Priority} | {t.PlannedTime:F1}h/{t.ActualTime:F1}h | Labels: {t.CategoryLabels})");

                txt.Text = sb.ToString();
            }

            cmbStories.SelectedIndexChanged += (s, e) => RefreshStoryReport();
            RefreshStoryReport();

            tp.Controls.Add(layout);
            return tp;
        }

        // Person Report

        private TabPage BuildPersonReportTab()
        {
            var tp = new TabPage("Person");

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var cmbPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(250, 250, 249),
                Padding = new Padding(6, 5, 6, 5),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            var cmbPersons = new ComboBox
            {
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10f)
            };
            var allPersons = _projectCtrl.GetAllPersons();
            cmbPersons.DataSource = allPersons;
            cmbPersons.DisplayMember = "Name";
            cmbPanel.Controls.Add(cmbPersons);
            layout.Controls.Add(cmbPanel, 0, 0);

            var txt = MakeReportTextBox();
            layout.Controls.Add(txt, 0, 1);

            void RefreshPersonReport()
            {
                if (cmbPersons.SelectedItem is not Person person) return;
                var sb = new StringBuilder();
                sb.AppendLine($"PERSON REPORT: {person.Name}");
                sb.AppendLine($"Role: {person.Role}");
                sb.AppendLine(new string('─', 50));

                var projects = _projectCtrl.GetAllProjects()
                    .Where(p => _projectCtrl.GetPersonsByProject(p.ProjectId)
                        .Any(x => x.PersonId == person.PersonId));

                foreach (var proj in projects)
                {
                    sb.AppendLine($"\nProject: {proj.Name}");
                    var stories = _storyCtrl.GetByProject(proj.ProjectId);
                    bool hasTasks = false;
                    foreach (var story in stories)
                    {
                        var tasks = _taskCtrl.GetByUserStory(story.UserStoryId);
                        foreach (var t in tasks)
                        {
                            sb.AppendLine($"  [{TaskStateLabel(t.State)}] {t.Title}  (Story: {story.Title})");
                            hasTasks = true;
                        }
                    }
                    if (!hasTasks) sb.AppendLine("  (no tasks)");
                }

                txt.Text = sb.ToString();
            }

            cmbPersons.SelectedIndexChanged += (s, e) => RefreshPersonReport();
            RefreshPersonReport();

            tp.Controls.Add(layout);
            return tp;
        }

        // Burndown Chart

        private TabPage BuildBurndownTab()
        {
            var tp = new TabPage("Burndown Chart");

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Top bar: label + info
            var topBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(250, 250, 249),
                Padding = new Padding(8, 8, 8, 8),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            var lblInfo = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 76),
                Text = "Sprint burndown — tasks remaining over time"
            };
            topBar.Controls.Add(lblInfo);
            layout.Controls.Add(topBar, 0, 0);

            // Chart
            var chart = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };

            var chartArea = new ChartArea("main")
            {
                BackColor = Color.White,
                AxisX =
                {
                    Title = "Date",
                    TitleFont = new Font("Segoe UI", 8f),
                    LabelStyle = { Format = "MM/dd", Angle = -30, Font = new Font("Segoe UI", 7.5f) },
                    MajorGrid = { LineColor = Color.FromArgb(230, 228, 224) },
                    LineColor = Color.FromArgb(180, 178, 170)
                },
                AxisY =
                {
                    Title = "Tasks remaining",
                    TitleFont = new Font("Segoe UI", 8f),
                    LabelStyle = { Font = new Font("Segoe UI", 7.5f) },
                    MajorGrid = { LineColor = Color.FromArgb(230, 228, 224) },
                    LineColor = Color.FromArgb(180, 178, 170),
                    Minimum = 0
                }
            };
            chart.ChartAreas.Add(chartArea);

            // Legend
            chart.Legends.Add(new Legend("main")
            {
                Docking = Docking.Bottom,
                Font = new Font("Segoe UI", 8f),
                BackColor = Color.White
            });

            // Ideal line series
            var idealSeries = new Series("Ideal")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(180, 178, 170),
                BorderWidth = 2,
                BorderDashStyle = ChartDashStyle.Dash,
                ChartArea = "main",
                Legend = "main",
                XValueType = ChartValueType.DateTime
            };

            // Actual line series
            var actualSeries = new Series("Actual")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(83, 74, 183),
                BorderWidth = 3,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 6,
                ChartArea = "main",
                Legend = "main",
                XValueType = ChartValueType.DateTime
            };

            chart.Series.Add(idealSeries);
            chart.Series.Add(actualSeries);

            // Populate data
            PopulateBurndown(idealSeries, actualSeries, lblInfo);

            layout.Controls.Add(chart, 0, 1);
            tp.Controls.Add(layout);
            return tp;
        }

        private void PopulateBurndown(Series idealSeries, Series actualSeries, Label lblInfo)
        {
            // All tasks belonging to stories currently in sprint.
            var sprintStories = _storyCtrl.GetByProject(_project.ProjectId)
                .Where(s => s.State == UserStoryState.InSprint).ToList();

            var allTasks = sprintStories
                .SelectMany(s => _taskCtrl.GetByUserStory(s.UserStoryId))
                .ToList();

            if (allTasks.Count == 0)
            {
                lblInfo.Text = "No tasks in sprint yet.";
                lblInfo.ForeColor = Color.FromArgb(160, 45, 45);
                return;
            }

            // Sprint window: earliest planned start to latest planned end (fallback +/- 7 days from today).
            var tasksWithStart = allTasks.Where(t => t.PlannedStartDate.HasValue).ToList();
            var tasksWithEnd = allTasks.Where(t => t.PlannedEndDate.HasValue).ToList();

            DateTime startDate = tasksWithStart.Count > 0
                ? tasksWithStart.Min(t => t.PlannedStartDate!.Value.Date)
                : DateTime.Today.AddDays(-7);

            DateTime endDate = tasksWithEnd.Count > 0
                ? tasksWithEnd.Max(t => t.PlannedEndDate!.Value.Date)
                : DateTime.Today.AddDays(7);

            // Keep the chart at least one day wide and never end before today.
            if (endDate < DateTime.Today) endDate = DateTime.Today;
            if (endDate <= startDate) endDate = startDate.AddDays(1);

            int totalTasks = allTasks.Count;
            int totalDays = (int)(endDate - startDate).TotalDays;

            lblInfo.Text = $"Sprint burndown  |  {totalTasks} tasks  |  {startDate:dd/MM} → {endDate:dd/MM}";

            // Ideal line: straight drop from totalTasks to 0 across the sprint window.
            idealSeries.Points.AddXY(startDate.ToOADate(), totalTasks);
            idealSeries.Points.AddXY(endDate.ToOADate(), 0);

            // Actual line: tasks remaining = total - tasks with ActualEndDate <= day.
            // Plot only up to today; future days are not drawn.
            for (int i = 0; i <= totalDays; i++)
            {
                var day = startDate.AddDays(i);
                if (day > DateTime.Today) break;

                int completedByDay = allTasks.Count(t =>
                    t.ActualEndDate.HasValue && t.ActualEndDate.Value.Date <= day);

                int remaining = totalTasks - completedByDay;
                actualSeries.Points.AddXY(day.ToOADate(), remaining);
            }

            // Fallback if no task has been completed yet: show a single starting point.
            if (actualSeries.Points.Count == 0)
                actualSeries.Points.AddXY(startDate.ToOADate(), totalTasks);
        }

        // Helpers

        private RichTextBox MakeReportTextBox()
        {
            return new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9f),
                BackColor = Color.FromArgb(250, 250, 249),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                ForeColor = Color.FromArgb(40, 40, 38)
            };
        }

        private static string StateLabel(UserStoryState s) => s switch
        {
            UserStoryState.ProjectBacklog => "Backlog",
            UserStoryState.InSprint => "In Sprint",
            UserStoryState.Done => "Done",
            _ => s.ToString()
        };

        private static string TaskStateLabel(TaskState s) => s switch
        {
            TaskState.ToBeDone => "TODO",
            TaskState.InProcess => "IN PROGRESS",
            TaskState.Done => "DONE",
            _ => s.ToString()
        };
    }
}