using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Agile_Project.Controllers;
using Agile_Project.Models;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Forms
{
    public partial class MainForm : Form
    {
        private readonly ProjectController _projectCtrl = new();
        private readonly UserStoryController _storyCtrl = new();
        private readonly TaskController _taskCtrl = new();

        private List<Project> _projects = new();
        private Project? _selectedProject;

        // Topbar controls
        private ComboBox cmbProjects = new();
        private Button btnNewProject = new();
        private Button btnEditProject = new();
        private Button btnDeleteProject = new();
        private Button btnManagePersons = new();
        private Button btnReports = new();

        // Board columns
        private Panel pnlBacklog = new();
        private Panel pnlSprint = new();
        private Panel pnlDone = new();
        private FlowLayoutPanel flpBacklog = new();
        private FlowLayoutPanel flpSprint = new();
        private FlowLayoutPanel flpDone = new();

        public MainForm()
        {
            Text = "Agile Project Manager";
            Size = new Size(1100, 700);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 245, 243);
            Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Font; // responsive fix

            // --- Login trước khi build UI ---
            using var loginDlg = new LoginForm(_projectCtrl);
            if (loginDlg.ShowDialog() != DialogResult.OK)
            {
                Load += (s, e) => Close();
                return;
            }

            BuildUI();
            LoadProjects();

            // Resize → refresh để card width luôn đúng
            Resize += (s, e) => { if (_selectedProject != null) RefreshBoard(); };
        }

        private void BuildUI()
        {
            BuildTopbar();
            BuildBoard();
        }

        private void BuildTopbar()
        {
            var topbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(250, 250, 249),
                Padding = new Padding(0)
            };
            topbar.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 218, 213)),
                    0, topbar.Height - 1, topbar.Width, topbar.Height - 1);
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Padding = new Padding(8, 0, 8, 0),
                BackColor = Color.Transparent
            };

            var lblProject = new Label
            {
                Text = "Project:",
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 100, 96),
                Margin = new Padding(4, 14, 4, 0)
            };

            cmbProjects = new ComboBox
            {
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 10, 8, 0)
            };
            cmbProjects.SelectedIndexChanged += (s, e) =>
            {
                if (cmbProjects.SelectedItem is Project p)
                {
                    _selectedProject = p;
                    RefreshBoard();
                }
            };

            btnNewProject = MakeTopBtn("+ New Project", Color.FromArgb(83, 74, 183));
            btnNewProject.Click += BtnNewProject_Click;

            btnEditProject = MakeTopBtn("Edit", Color.FromArgb(60, 60, 58));
            btnEditProject.Click += BtnEditProject_Click;

            btnDeleteProject = MakeTopBtn("Delete", Color.FromArgb(160, 45, 45));
            btnDeleteProject.Click += BtnDeleteProject_Click;

            btnManagePersons = MakeTopBtn("Persons", Color.FromArgb(60, 60, 58));
            btnManagePersons.Click += BtnManagePersons_Click;

            btnReports = MakeTopBtn("Reports", Color.FromArgb(15, 110, 86));
            btnReports.Click += BtnReports_Click;

            // --- Ẩn button theo role ---
            btnNewProject.Visible = PermissionService.CanDo("ManageProject");
            btnEditProject.Visible = PermissionService.CanDo("ManageProject");
            btnDeleteProject.Visible = PermissionService.CanDo("ManageProject");
            btnManagePersons.Visible = PermissionService.CanDo("AssignPerson");
            // btnReports luôn hiện (ViewReport = true)

            // Label hiển thị user đang login
            var lblUser = new Label
            {
                Text = $"👤 {CurrentSession.Username} ({CurrentSession.Role})",
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 100, 96),
                Margin = new Padding(12, 14, 8, 0)
            };

            // Nút Logout
            var btnLogout = MakeTopBtn("Logout", Color.FromArgb(160, 45, 45));
            btnLogout.Margin = new Padding(0, 10, 8, 0);
            btnLogout.Click += BtnLogout_Click;

            flow.Controls.AddRange(new Control[] {
                lblProject, cmbProjects,
                btnNewProject, btnEditProject, btnDeleteProject,
                btnManagePersons, btnReports,
                lblUser, btnLogout
            });

            topbar.Controls.Add(flow);
            Controls.Add(topbar);
        }

        private Button MakeTopBtn(string text, Color foreColor)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                ForeColor = foreColor,
                BackColor = Color.FromArgb(250, 250, 249),
                Cursor = Cursors.Hand,
                Height = 28,
                Padding = new Padding(6, 0, 6, 0),
                Margin = new Padding(0, 10, 6, 0),
                FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193), BorderSize = 1 }
            };
        }

        private void BuildBoard()
        {
            var boardPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(245, 245, 243)
            };
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.4f));

            pnlBacklog = BuildColumn("Project Backlog", Color.FromArgb(83, 74, 183), out flpBacklog);
            pnlSprint = BuildColumn("In Sprint", Color.FromArgb(180, 90, 20), out flpSprint);
            pnlDone = BuildColumn("Done", Color.FromArgb(59, 109, 17), out flpDone);

            boardPanel.Controls.Add(pnlBacklog, 0, 0);
            boardPanel.Controls.Add(pnlSprint, 1, 0);
            boardPanel.Controls.Add(pnlDone, 2, 0);

            Controls.Add(boardPanel);
            Controls.SetChildIndex(boardPanel, 0);
        }

        private Panel BuildColumn(string title, Color accentColor, out FlowLayoutPanel flp)
        {
            var col = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 239, 235),
                Padding = new Padding(1),
                Margin = new Padding(4)
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = Color.FromArgb(248, 248, 246),
                Padding = new Padding(0)
            };
            header.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 218, 213)),
                    0, header.Height - 1, header.Width, header.Height - 1);
                e.Graphics.FillRectangle(new SolidBrush(accentColor), 0, 0, 3, header.Height);
            };

            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = accentColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };
            header.Controls.Add(lblTitle);

            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(240, 239, 235)
            };

            flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(8)
            };

            scroll.Controls.Add(flp);
            col.Controls.Add(scroll);
            col.Controls.Add(header);
            return col;
        }

        // ── Responsive card width ─────────────────────────────────────
        // Tính width dựa theo kích thước cột thực tế thay vì hardcode 260
        private int GetCardWidth()
        {
            int w = flpBacklog.ClientSize.Width - flpBacklog.Padding.Horizontal - 8;
            return Math.Max(200, w);
        }

        // ── Data loading ──────────────────────────────────────────────

        private void LoadProjects()
        {
            _projects = _projectCtrl.GetAllProjects();
            cmbProjects.DataSource = null;
            cmbProjects.DataSource = _projects;
            cmbProjects.DisplayMember = "Name";

            if (_projects.Count > 0)
            {
                _selectedProject = _projects[0];
                cmbProjects.SelectedIndex = 0;
                RefreshBoard();
            }
            else
            {
                ClearBoard();
            }
        }

        public void RefreshBoard()
        {
            if (_selectedProject == null) return;
            ClearBoard();

            int cardW = GetCardWidth();

            var stories = _storyCtrl.GetByProject(_selectedProject.ProjectId);
            foreach (var story in stories)
            {
                switch (story.State)
                {
                    case UserStoryState.ProjectBacklog:
                        flpBacklog.Controls.Add(MakeBacklogCard(story, cardW));
                        break;
                    case UserStoryState.InSprint:
                        flpSprint.Controls.Add(MakeSprintCard(story, cardW));
                        break;
                    case UserStoryState.Done:
                        flpDone.Controls.Add(MakeDoneCard(story, cardW));
                        break;
                }
            }

            // "+ Add User Story" button — chỉ hiện nếu có quyền
            if (PermissionService.CanDo("ManageUserStory"))
            {
                var btnAdd = new Button
                {
                    Text = "+ Add User Story",
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(100, 100, 96),
                    BackColor = Color.FromArgb(240, 239, 235),
                    Width = cardW,
                    Height = 30,
                    Cursor = Cursors.Hand,
                    FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193), BorderSize = 1 }
                };
                btnAdd.Click += (s, e) => BtnAddStory_Click();
                flpBacklog.Controls.Add(btnAdd);
            }

            flpBacklog.Height = flpBacklog.GetPreferredSize(Size.Empty).Height;
            flpSprint.Height = flpSprint.GetPreferredSize(Size.Empty).Height;
            flpDone.Height = flpDone.GetPreferredSize(Size.Empty).Height;
        }

        private void ClearBoard()
        {
            flpBacklog.Controls.Clear();
            flpSprint.Controls.Clear();
            flpDone.Controls.Clear();
        }

        // ── Card builders ─────────────────────────────────────────────

        private Control MakeBacklogCard(UserStory story, int cardW)
        {
            var card = MakeCardBase(cardW);
            int y = 10;

            var lblTitle = new Label
            {
                Text = story.Title,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 38),
                Location = new Point(10, y),
                Width = cardW - 20,
                AutoSize = false,
                Height = 20
            };
            card.Controls.Add(lblTitle);
            y += 22;

            var tasks = _taskCtrl.GetByUserStory(story.UserStoryId);
            var lblMeta = new Label
            {
                Text = $"Priority: {story.Priority}  ·  {tasks.Count} tasks",
                ForeColor = Color.FromArgb(130, 128, 122),
                Location = new Point(10, y),
                Width = cardW - 20,
                Height = 18,
                Font = new Font("Segoe UI", 8f)
            };
            card.Controls.Add(lblMeta);
            y += 24;

            // Buttons: chỉ hiện theo quyền
            int btnX = 10;
            if (PermissionService.CanDo("ManageUserStory"))
            {
                var btnMove = MakeCardBtn("→ Move to Sprint", Color.FromArgb(83, 74, 183));
                btnMove.Location = new Point(btnX, y);
                btnMove.Click += (s, e) => MoveStory(story, UserStoryState.InSprint);
                card.Controls.Add(btnMove);
                btnX = btnMove.Right + 4;
            }

            if (PermissionService.CanDo("AddTask"))
            {
                var btnAddTask = MakeCardBtn("+ Task", Color.FromArgb(60, 60, 58));
                btnAddTask.Location = new Point(btnX, y);
                btnAddTask.Click += (s, e) => AddTask(story);
                card.Controls.Add(btnAddTask);
            }
            y += 28;

            if (PermissionService.CanDo("ManageUserStory"))
            {
                var btnEdit = MakeCardBtn("Edit", Color.FromArgb(60, 60, 58));
                btnEdit.Location = new Point(10, y);
                btnEdit.Click += (s, e) => EditStory(story);
                card.Controls.Add(btnEdit);

                var btnDel = MakeCardBtn("Delete", Color.FromArgb(160, 45, 45));
                btnDel.Location = new Point(btnEdit.Right + 4, y);
                btnDel.Click += (s, e) => DeleteStory(story);
                card.Controls.Add(btnDel);
                y += 30;
            }

            card.Height = y + 6;
            return card;
        }

        private Control MakeSprintCard(UserStory story, int cardW)
        {
            var card = MakeCardBase(cardW);
            int y = 10;

            var lblTitle = new Label
            {
                Text = story.Title,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 38),
                Location = new Point(10, y),
                Width = cardW - 20,
                AutoSize = false,
                Height = 20
            };
            card.Controls.Add(lblTitle);
            y += 22;

            var tasks = _taskCtrl.GetByUserStory(story.UserStoryId);

            if (tasks.Count > 0)
            {
                var sep = new Panel
                {
                    Location = new Point(10, y),
                    Width = cardW - 20,
                    Height = 1,
                    BackColor = Color.FromArgb(220, 218, 213)
                };
                card.Controls.Add(sep);
                y += 8;

                foreach (var task in tasks)
                {
                    var assignedPersons = _taskCtrl.GetById(task.TaskId) != null
                        ? GetAssignedPersonNames(task.TaskId) : "";
                    var taskRow = MakeTaskRow(task, story, assignedPersons, cardW, ref y);
                    card.Controls.Add(taskRow);
                    y += taskRow.Height + 3;
                }

                var sep2 = new Panel
                {
                    Location = new Point(10, y),
                    Width = cardW - 20,
                    Height = 1,
                    BackColor = Color.FromArgb(220, 218, 213)
                };
                card.Controls.Add(sep2);
                y += 8;
            }

            // Buttons theo quyền
            int btnX = 10;
            if (PermissionService.CanDo("ManageUserStory"))
            {
                var btnDone = MakeCardBtn("→ Mark Done", Color.FromArgb(15, 110, 86));
                btnDone.Location = new Point(btnX, y);
                btnDone.Click += (s, e) => MoveStory(story, UserStoryState.Done);
                card.Controls.Add(btnDone);

                var btnBack = MakeCardBtn("← Back", Color.FromArgb(100, 100, 96));
                btnBack.Location = new Point(btnDone.Right + 4, y);
                btnBack.Click += (s, e) => MoveStory(story, UserStoryState.ProjectBacklog);
                card.Controls.Add(btnBack);
                y += 28;
                btnX = 10;
            }

            if (PermissionService.CanDo("AddTask"))
            {
                var btnAddTask = MakeCardBtn("+ Task", Color.FromArgb(60, 60, 58));
                btnAddTask.Location = new Point(btnX, y);
                btnAddTask.Click += (s, e) => AddTask(story);
                card.Controls.Add(btnAddTask);
                btnX = btnAddTask.Right + 4;
            }

            var btnReport = MakeCardBtn("Report", Color.FromArgb(60, 60, 58));
            btnReport.Location = new Point(btnX, y);
            btnReport.Click += (s, e) => ShowStoryReport(story);
            card.Controls.Add(btnReport);
            y += 30;

            card.Height = y + 6;
            return card;
        }

        private Control MakeDoneCard(UserStory story, int cardW)
        {
            var card = MakeCardBase(cardW);
            card.BackColor = Color.FromArgb(248, 250, 246);
            int y = 10;

            var lblTitle = new Label
            {
                Text = story.Title,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 130, 80),
                Location = new Point(10, y),
                Width = cardW - 20,
                AutoSize = false,
                Height = 20
            };
            card.Controls.Add(lblTitle);
            y += 22;

            var tasks = _taskCtrl.GetByUserStory(story.UserStoryId);
            int doneTasks = tasks.Count(t => t.State == TaskState.Done);
            var lblMeta = new Label
            {
                Text = $"Priority: {story.Priority}  ·  {doneTasks}/{tasks.Count} tasks done",
                ForeColor = Color.FromArgb(130, 155, 110),
                Location = new Point(10, y),
                Width = cardW - 20,
                Height = 18,
                Font = new Font("Segoe UI", 8f)
            };
            card.Controls.Add(lblMeta);
            y += 24;

            int btnX = 10;
            if (PermissionService.CanDo("ManageUserStory"))
            {
                var btnBack = MakeCardBtn("← Back to Sprint", Color.FromArgb(100, 100, 96));
                btnBack.Location = new Point(btnX, y);
                btnBack.Click += (s, e) => MoveStory(story, UserStoryState.InSprint);
                card.Controls.Add(btnBack);
                btnX = btnBack.Right + 4;
            }

            var btnReport = MakeCardBtn("Report", Color.FromArgb(60, 60, 58));
            btnReport.Location = new Point(btnX, y);
            btnReport.Click += (s, e) => ShowStoryReport(story);
            card.Controls.Add(btnReport);
            y += 30;

            card.Height = y + 6;
            return card;
        }

        // Width giờ là tham số thay vì hardcode 260
        private Panel MakeCardBase(int width)
        {
            return new Panel
            {
                Width = width,
                Height = 120,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(0),
                BorderStyle = BorderStyle.None
            };
        }

        private Panel MakeTaskRow(ProjectTask task, UserStory story, string persons, int cardWidth, ref int y)
        {
            var row = new Panel
            {
                Location = new Point(10, y),
                Width = cardWidth - 20,
                Height = 22
            };

            Color dotColor = task.State switch
            {
                TaskState.Done => Color.FromArgb(99, 153, 34),
                TaskState.InProcess => Color.FromArgb(239, 159, 39),
                _ => Color.FromArgb(180, 178, 170)
            };

            var dot = new Panel
            {
                Size = new Size(10, 10),
                Location = new Point(0, 6),
                BackColor = dotColor
            };
            MakeRound(dot);

            // Chỉ cho click cycle nếu có quyền ChangeTaskState
            if (PermissionService.CanDo("ChangeTaskState"))
            {
                dot.Cursor = Cursors.Hand;
                dot.Click += (s, e) => CycleTaskState(task, story);
            }
            row.Controls.Add(dot);

            var lblTask = new Label
            {
                Text = task.Title,
                Location = new Point(16, 3),
                Width = row.Width - 70,
                Height = 18,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(60, 60, 58)
            };
            row.Controls.Add(lblTask);

            var lblPerson = new Label
            {
                Text = persons,
                Location = new Point(row.Width - 52, 3),
                Width = 50,
                Height = 18,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(130, 128, 122),
                TextAlign = ContentAlignment.MiddleRight
            };
            row.Controls.Add(lblPerson);

            var btnDetail = new Label
            {
                Text = "...",
                Location = new Point(row.Width - 18, 2),
                Width = 18,
                Height = 18,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(150, 148, 140),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8f)
            };
            btnDetail.Click += (s, e) => OpenTaskDetail(task, story);
            row.Controls.Add(btnDetail);

            return row;
        }

        private void MakeRound(Panel p)
        {
            p.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, p.Width, p.Height, p.Width, p.Height));
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        private Button MakeCardBtn(string text, Color foreColor)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                Height = 24,
                FlatStyle = FlatStyle.Flat,
                ForeColor = foreColor,
                BackColor = Color.FromArgb(248, 248, 246),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 7.5f),
                Padding = new Padding(4, 0, 4, 0),
                FlatAppearance = { BorderColor = Color.FromArgb(210, 208, 203), BorderSize = 1 }
            };
        }

        private string GetAssignedPersonNames(int taskId) => "";

        // ── Actions ───────────────────────────────────────────────────

        private void MoveStory(UserStory story, UserStoryState newState)
        {
            var (ok, msg) = _storyCtrl.ChangeState(story.UserStoryId, newState);
            if (!ok) MessageBox.Show(msg, "Cannot move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RefreshBoard();
        }

        private void CycleTaskState(ProjectTask task, UserStory story)
        {
            TaskState next = task.State switch
            {
                TaskState.ToBeDone => TaskState.InProcess,
                TaskState.InProcess => TaskState.Done,
                TaskState.Done => TaskState.InProcess,
                _ => task.State
            };
            var (ok, msg) = _taskCtrl.ChangeState(task.TaskId, next);
            if (!ok) MessageBox.Show(msg, "Cannot change state", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RefreshBoard();
        }

        private void AddTask(UserStory story)
        {
            var dlg = new TaskDetailDialog(story, null, _projectCtrl, _taskCtrl);
            if (dlg.ShowDialog() == DialogResult.OK) RefreshBoard();
        }

        private void OpenTaskDetail(ProjectTask task, UserStory story)
        {
            var dlg = new TaskDetailDialog(story, task, _projectCtrl, _taskCtrl);
            if (dlg.ShowDialog() == DialogResult.OK) RefreshBoard();
        }

        private void EditStory(UserStory story)
        {
            var dlg = new AddEditUserStoryDialog(story, _selectedProject!.ProjectId, _storyCtrl);
            if (dlg.ShowDialog() == DialogResult.OK) RefreshBoard();
        }

        private void DeleteStory(UserStory story)
        {
            var r = MessageBox.Show($"Delete story \"{story.Title}\" and all its tasks?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r == DialogResult.Yes)
            {
                _storyCtrl.DeleteUserStory(story.UserStoryId);
                RefreshBoard();
            }
        }

        private void ShowStoryReport(UserStory story)
        {
            var dlg = new ReportsForm(_selectedProject!, story, _storyCtrl, _taskCtrl, _projectCtrl);
            dlg.ShowDialog();
        }

        private void BtnAddStory_Click()
        {
            if (_selectedProject == null) return;
            var dlg = new AddEditUserStoryDialog(null, _selectedProject.ProjectId, _storyCtrl);
            if (dlg.ShowDialog() == DialogResult.OK) RefreshBoard();
        }

        private void BtnNewProject_Click(object? s, EventArgs e)
        {
            var dlg = new AddEditProjectDialog(null, _projectCtrl);
            if (dlg.ShowDialog() == DialogResult.OK) LoadProjects();
        }

        private void BtnEditProject_Click(object? s, EventArgs e)
        {
            if (_selectedProject == null) return;
            var dlg = new AddEditProjectDialog(_selectedProject, _projectCtrl);
            if (dlg.ShowDialog() == DialogResult.OK) LoadProjects();
        }

        private void BtnDeleteProject_Click(object? s, EventArgs e)
        {
            if (_selectedProject == null) return;
            var r = MessageBox.Show($"Delete project \"{_selectedProject.Name}\"?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r == DialogResult.Yes)
            {
                _projectCtrl.DeleteProject(_selectedProject.ProjectId);
                LoadProjects();
            }
        }

        private void BtnManagePersons_Click(object? s, EventArgs e)
        {
            if (_selectedProject == null) return;
            var dlg = new ManagePersonsDialog(_selectedProject, _projectCtrl);
            dlg.ShowDialog();
        }

        private void BtnReports_Click(object? s, EventArgs e)
        {
            if (_selectedProject == null) return;
            var dlg = new ReportsForm(_selectedProject, null, _storyCtrl, _taskCtrl, _projectCtrl);
            dlg.ShowDialog();
        }

        private void BtnLogout_Click(object? s, EventArgs e)
        {
            // Clear session
            CurrentSession.PersonId = 0;
            CurrentSession.Username = "";
            CurrentSession.Role = "";

            // Show login — nếu cancel thì thoát app, nếu OK thì rebuild UI tại chỗ
            using var loginDlg = new LoginForm(_projectCtrl);
            if (loginDlg.ShowDialog() != DialogResult.OK)
            {
                Application.Exit();
                return;
            }

            // Rebuild UI ngay trên form này, không tạo MainForm mới
            Controls.Clear();
            BuildUI();
            LoadProjects();
        }
    }
}