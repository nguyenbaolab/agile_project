using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Agile_Project.Controllers;
using Agile_Project.Models;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Forms
{
    public class ViewTasksForm : Form
    {
        private readonly int _userStoryId;
        private readonly ITaskController _taskCtrl;
        private readonly IProjectController _projectCtrl;
        private readonly IUserStoryController _storyCtrl;
        private readonly int _projectId;

        // Controls
        private ComboBox cboTasks = new();
        private TextBox txtTitle = new();
        private NumericUpDown numPriority = new();
        private NumericUpDown numDifficulty = new();
        private TextBox txtLabels = new();
        private NumericUpDown numPlannedTime = new();
        private NumericUpDown numActualTime = new();
        private CheckBox chkPlannedStart = new();
        private CheckBox chkPlannedEnd = new();
        private CheckBox chkActualStart = new();
        private CheckBox chkActualEnd = new();
        private DateTimePicker dtpPlannedStart = new();
        private DateTimePicker dtpPlannedEnd = new();
        private DateTimePicker dtpActualStart = new();
        private DateTimePicker dtpActualEnd = new();
        private ComboBox cmbState = new();
        private ListBox lstAssigned = new();
        private ComboBox cmbPersonAssign = new();
        private ListBox lstAssignedTeams = new();
        private ComboBox cmbTeamAssign = new();
        private Label lblStatus = new();

        // Keeps track of which task is currently displayed
        private ProjectTask? _currentTask;

        // Dev sees this dialog as read-only. Only Admin/PO (AddTask permission) can edit task details.
        private readonly bool _canEditDetails = PermissionService.CanDo("AddTask");

        // Holds the story-state bar so it can be rebuilt after a state change.
        private Panel _storyStateBar = new();

        public ViewTasksForm(int userStoryId, string storyTitle,
            ITaskController taskCtrl, IProjectController projectCtrl,
            IUserStoryController storyCtrl, int projectId)
        {
            _userStoryId = userStoryId;
            _taskCtrl = taskCtrl;
            _projectCtrl = projectCtrl;
            _storyCtrl = storyCtrl;
            _projectId = projectId;

            Text = $"Tasks of: {storyTitle}";
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = new Size(620, 720);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Font;
            Padding = new Padding(16, 12, 16, 12);

            BuildUI();
            LoadTasks();
            LoadProjectPersons();
            LoadProjectTeams();
        }

        // UI Construction

        private void BuildUI()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };

            var layout = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                ColumnCount = 1,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Story-state bar: parent story state + transition buttons (Dev/PO/Admin).
            // Sits at the top so state moves are obvious before drilling into tasks.
            _storyStateBar = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 10)
            };
            layout.Controls.Add(_storyStateBar);
            BuildStoryStateBar();

            layout.Controls.Add(MakeSeparator());

            // Task selector
            layout.Controls.Add(MakeLabel("Select task:"));
            cboTasks = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 12)
            };
            cboTasks.SelectedIndexChanged += CboTasks_SelectedIndexChanged;
            layout.Controls.Add(cboTasks);

            layout.Controls.Add(MakeSeparator());

            // Title
            layout.Controls.Add(MakeLabel("Title *"));
            txtTitle = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 10)
            };
            layout.Controls.Add(txtTitle);

            // Priority / Difficulty
            layout.Controls.Add(MakeLabel("Priority  /  Difficulty"));
            var rowPD = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 10)
            };
            numPriority = MakeNum(0, 100);
            numPriority.Margin = new Padding(0, 0, 50, 0);
            numDifficulty = MakeNum(0, 10);
            rowPD.Controls.Add(numPriority);
            rowPD.Controls.Add(numDifficulty);
            layout.Controls.Add(rowPD);

            // Category Labels
            layout.Controls.Add(MakeLabel("Category labels (comma separated)"));
            txtLabels = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 10)
            };
            layout.Controls.Add(txtLabels);

            // Planned / Actual time
            layout.Controls.Add(MakeLabel("Planned time (h)  /  Actual time (h)"));
            var rowTimes = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 10)
            };
            numPlannedTime = MakeDecimalNum();
            numPlannedTime.Margin = new Padding(0, 0, 50, 0);
            numActualTime = MakeDecimalNum();
            rowTimes.Controls.Add(numPlannedTime);
            rowTimes.Controls.Add(numActualTime);
            layout.Controls.Add(rowTimes);

            // Dates
            layout.Controls.Add(MakeLabel("Dates (check to enable)"));
            var rowDates = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 10)
            };
            rowDates.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            rowDates.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            chkPlannedStart = MakeChk("Planned start"); dtpPlannedStart = MakeDtp();
            chkPlannedStart.CheckedChanged += (s, e) => dtpPlannedStart.Enabled = chkPlannedStart.Checked;
            rowDates.Controls.Add(chkPlannedStart); rowDates.Controls.Add(dtpPlannedStart);

            chkPlannedEnd = MakeChk("Planned end"); dtpPlannedEnd = MakeDtp();
            chkPlannedEnd.CheckedChanged += (s, e) => dtpPlannedEnd.Enabled = chkPlannedEnd.Checked;
            rowDates.Controls.Add(chkPlannedEnd); rowDates.Controls.Add(dtpPlannedEnd);

            chkActualStart = MakeChk("Actual start"); dtpActualStart = MakeDtp();
            chkActualStart.CheckedChanged += (s, e) => dtpActualStart.Enabled = chkActualStart.Checked;
            rowDates.Controls.Add(chkActualStart); rowDates.Controls.Add(dtpActualStart);

            chkActualEnd = MakeChk("Actual end"); dtpActualEnd = MakeDtp();
            chkActualEnd.CheckedChanged += (s, e) => dtpActualEnd.Enabled = chkActualEnd.Checked;
            rowDates.Controls.Add(chkActualEnd); rowDates.Controls.Add(dtpActualEnd);

            layout.Controls.Add(rowDates);

            // State
            layout.Controls.Add(MakeLabel("State"));
            cmbState = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 10)
            };
            cmbState.Items.AddRange(new object[] { "ToBeDone", "InProcess", "Done" });
            cmbState.SelectedIndex = 0;
            // State combo: shown to anyone who can edit details (Admin/PO) AND has ChangeTaskState.
            // Dev keeps task-state moves on the board's colored dot, not in this dialog.
            cmbState.Enabled = _canEditDetails && PermissionService.CanDo("ChangeTaskState");
            layout.Controls.Add(cmbState);

            layout.Controls.Add(MakeSeparator());

            // Assigned persons
            if (PermissionService.CanDo("AssignPerson"))
            {
                layout.Controls.Add(MakeLabel("Assigned persons"));

                var rowPersons = new TableLayoutPanel
                {
                    AutoSize = true,
                    ColumnCount = 2,
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 0, 6),
                    Dock = DockStyle.Fill
                };
                rowPersons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                rowPersons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

                lstAssigned = new ListBox
                {
                    Dock = DockStyle.Fill,
                    Height = 90,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(0, 0, 6, 0),
                    DisplayMember = "Name"
                };
                rowPersons.Controls.Add(lstAssigned, 0, 0);

                var btnColPersons = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    BackColor = Color.White
                };
                var btnRemove = new Button
                {
                    Text = "Remove",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(160, 45, 45),
                    Margin = new Padding(0, 0, 0, 4),
                    FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
                };
                btnRemove.Click += BtnRemovePerson_Click;
                btnColPersons.Controls.Add(btnRemove);
                rowPersons.Controls.Add(btnColPersons, 1, 0);
                layout.Controls.Add(rowPersons);

                // Assign row
                var rowAssign = new TableLayoutPanel
                {
                    AutoSize = true,
                    ColumnCount = 2,
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 0, 12),
                    Dock = DockStyle.Fill
                };
                rowAssign.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                rowAssign.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

                cmbPersonAssign = new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(0, 0, 6, 0),
                    DisplayMember = "Name",
                    ValueMember = "PersonId"
                };
                rowAssign.Controls.Add(cmbPersonAssign, 0, 0);

                var btnAssign = new Button
                {
                    Text = "Assign",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(83, 74, 183),
                    ForeColor = Color.White,
                    FlatAppearance = { BorderSize = 0 }
                };
                btnAssign.Click += BtnAssignPerson_Click;
                rowAssign.Controls.Add(btnAssign, 1, 0);
                layout.Controls.Add(rowAssign);

                // Assigned teams: same shape as the Assigned persons block above.
                // The dropdown lists only this project's teams; the controller revalidates on assign.
                layout.Controls.Add(MakeLabel("Assigned teams"));

                var rowTeams = new TableLayoutPanel
                {
                    AutoSize = true,
                    ColumnCount = 2,
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 0, 6),
                    Dock = DockStyle.Fill
                };
                rowTeams.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                rowTeams.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

                lstAssignedTeams = new ListBox
                {
                    Dock = DockStyle.Fill,
                    Height = 90,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(0, 0, 6, 0),
                    DisplayMember = "Name"
                };
                rowTeams.Controls.Add(lstAssignedTeams, 0, 0);

                var btnColTeams = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    BackColor = Color.White
                };
                var btnRemoveTeam = new Button
                {
                    Text = "Remove",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(160, 45, 45),
                    Margin = new Padding(0, 0, 0, 4),
                    FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
                };
                btnRemoveTeam.Click += BtnRemoveTeam_Click;
                btnColTeams.Controls.Add(btnRemoveTeam);
                rowTeams.Controls.Add(btnColTeams, 1, 0);
                layout.Controls.Add(rowTeams);

                var rowAssignTeam = new TableLayoutPanel
                {
                    AutoSize = true,
                    ColumnCount = 2,
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 0, 12),
                    Dock = DockStyle.Fill
                };
                rowAssignTeam.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                rowAssignTeam.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

                cmbTeamAssign = new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(0, 0, 6, 0),
                    DisplayMember = "Name",
                    ValueMember = "TeamId"
                };
                rowAssignTeam.Controls.Add(cmbTeamAssign, 0, 0);

                var btnAssignTeam = new Button
                {
                    Text = "Assign",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(83, 74, 183),
                    ForeColor = Color.White,
                    FlatAppearance = { BorderSize = 0 }
                };
                btnAssignTeam.Click += BtnAssignTeam_Click;
                rowAssignTeam.Controls.Add(btnAssignTeam, 1, 0);
                layout.Controls.Add(rowAssignTeam);
            }

            layout.Controls.Add(MakeSeparator());

            // Status label
            lblStatus = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(15, 110, 86),
                Margin = new Padding(0, 0, 0, 6),
                Text = ""
            };
            layout.Controls.Add(lblStatus);

            // Buttons
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            var btnClose = new Button
            {
                Text = "Close",
                AutoSize = true,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 0, 0, 0),
                FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
            };
            btnPanel.Controls.Add(btnClose);

            // Save Changes is only shown for users who can edit task details (Admin/PO).
            if (_canEditDetails)
            {
                var btnSave = new Button
                {
                    Text = "Save Changes",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(83, 74, 183),
                    ForeColor = Color.White,
                    Margin = new Padding(6, 0, 0, 0),
                    FlatAppearance = { BorderSize = 0 }
                };
                btnSave.Click += BtnSaveChanges_Click;
                btnPanel.Controls.Add(btnSave);
            }

            layout.Controls.Add(btnPanel);

            scroll.Controls.Add(layout);
            Controls.Add(scroll);
            CancelButton = btnClose;
        }

        // Rebuilds the story-state bar based on the parent story's current state.
        // Buttons mirror the board card buttons and are gated on ChangeUserStoryState.
        private void BuildStoryStateBar()
        {
            _storyStateBar.Controls.Clear();

            var story = _storyCtrl.GetById(_userStoryId);
            if (story == null) return;

            var lblState = new Label
            {
                Text = "Story state: " + story.State,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 58),
                Location = new Point(0, 6)
            };
            _storyStateBar.Controls.Add(lblState);

            if (!PermissionService.CanDo("ChangeUserStoryState")) return;

            int btnX = lblState.Right + 16;
            void Add(string text, Color color, UserStoryState target)
            {
                var b = new Button
                {
                    Text = text,
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = color,
                    ForeColor = Color.White,
                    Location = new Point(btnX, 0),
                    FlatAppearance = { BorderSize = 0 },
                    Cursor = Cursors.Hand
                };
                b.Click += (_, _) => MoveStory(target);
                _storyStateBar.Controls.Add(b);
                btnX = b.Right + 6;
            }

            switch (story.State)
            {
                case UserStoryState.ProjectBacklog:
                    Add("→ Move to Sprint", Color.FromArgb(83, 74, 183), UserStoryState.InSprint);
                    break;
                case UserStoryState.InSprint:
                    Add("→ Mark Done", Color.FromArgb(15, 110, 86), UserStoryState.Done);
                    Add("← Back", Color.FromArgb(100, 100, 96), UserStoryState.ProjectBacklog);
                    break;
                case UserStoryState.Done:
                    Add("← Back to Sprint", Color.FromArgb(100, 100, 96), UserStoryState.InSprint);
                    break;
            }
        }

        private void MoveStory(UserStoryState newState)
        {
            var (ok, msg) = _storyCtrl.ChangeState(_userStoryId, newState);
            if (!ok)
            {
                MessageBox.Show(msg, "Cannot move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Story state may reset task states (e.g. back to backlog -> all tasks ToBeDone).
            BuildStoryStateBar();
            LoadTasks();
        }

        // Data loading

        private void LoadTasks()
        {
            var tasks = _taskCtrl.GetByUserStory(_userStoryId);
            cboTasks.DataSource = null;
            cboTasks.DataSource = tasks;
            cboTasks.DisplayMember = "Title";
            cboTasks.ValueMember = "TaskId";

            SetFieldsEnabled(tasks.Count > 0);
        }

        private void LoadProjectPersons()
        {
            var persons = _projectCtrl.GetPersonsByProject(_projectId)
                .Where(p => !string.Equals(p.ProfileRole, "Admin", StringComparison.OrdinalIgnoreCase))
                .ToList();
            cmbPersonAssign.DataSource = null;
            cmbPersonAssign.DataSource = persons;
            cmbPersonAssign.DisplayMember = "Name";
            cmbPersonAssign.ValueMember = "PersonId";
        }

        // Fill the team-assign dropdown with this project's teams only.
        private void LoadProjectTeams()
        {
            var teams = _taskCtrl.GetProjectTeams(_projectId);
            cmbTeamAssign.DataSource = null;
            cmbTeamAssign.DataSource = teams;
            cmbTeamAssign.DisplayMember = "Name";
            cmbTeamAssign.ValueMember = "TeamId";
        }

        private void LoadSelectedTask()
        {
            if (cboTasks.SelectedItem is not ProjectTask task)
            {
                _currentTask = null;
                SetFieldsEnabled(false);
                return;
            }

            _currentTask = task;
            SetFieldsEnabled(true);

            // Populate fields
            txtTitle.Text = task.Title;
            numPriority.Value = task.Priority;
            numDifficulty.Value = task.Difficulty;
            txtLabels.Text = task.CategoryLabels;
            numPlannedTime.Value = (decimal)task.PlannedTime;
            numActualTime.Value = (decimal)task.ActualTime;

            chkPlannedStart.Checked = task.PlannedStartDate.HasValue;
            if (task.PlannedStartDate.HasValue) dtpPlannedStart.Value = task.PlannedStartDate.Value;

            chkPlannedEnd.Checked = task.PlannedEndDate.HasValue;
            if (task.PlannedEndDate.HasValue) dtpPlannedEnd.Value = task.PlannedEndDate.Value;

            chkActualStart.Checked = task.ActualStartDate.HasValue;
            if (task.ActualStartDate.HasValue) dtpActualStart.Value = task.ActualStartDate.Value;

            chkActualEnd.Checked = task.ActualEndDate.HasValue;
            if (task.ActualEndDate.HasValue) dtpActualEnd.Value = task.ActualEndDate.Value;

            // State
            if (cmbState.Items.Count > 0)
                cmbState.SelectedIndex = (int)task.State;

            RefreshAssignedPersons();
            RefreshAssignedTeams();
            lblStatus.Text = "";
        }

        private void RefreshAssignedPersons()
        {
            if (_currentTask == null) return;
            var assigned = _taskCtrl.GetAssignedPersons(_currentTask.TaskId);
            lstAssigned.DataSource = null;
            lstAssigned.DataSource = assigned;
            lstAssigned.DisplayMember = "Name";
        }

        private void RefreshAssignedTeams()
        {
            if (_currentTask == null) return;
            var assigned = _taskCtrl.GetAssignedTeams(_currentTask.TaskId);
            lstAssignedTeams.DataSource = null;
            lstAssignedTeams.DataSource = assigned;
            lstAssignedTeams.DisplayMember = "Name";
        }

        private void SetFieldsEnabled(bool enabled)
        {
            // Dev (no AddTask permission) sees fields filled but read-only.
            bool edit = enabled && _canEditDetails;

            txtTitle.Enabled = edit;
            numPriority.Enabled = edit;
            numDifficulty.Enabled = edit;
            txtLabels.Enabled = edit;
            numPlannedTime.Enabled = edit;
            numActualTime.Enabled = edit;
            chkPlannedStart.Enabled = edit;
            chkPlannedEnd.Enabled = edit;
            chkActualStart.Enabled = edit;
            chkActualEnd.Enabled = edit;
            // Date pickers follow their checkbox state, not enabled flag
            if (!edit)
            {
                dtpPlannedStart.Enabled = false;
                dtpPlannedEnd.Enabled = false;
                dtpActualStart.Enabled = false;
                dtpActualEnd.Enabled = false;
            }
            cmbState.Enabled = edit && PermissionService.CanDo("ChangeTaskState");
            lstAssigned.Enabled = enabled;
            cmbPersonAssign.Enabled = enabled;
            lstAssignedTeams.Enabled = enabled;
            cmbTeamAssign.Enabled = enabled;
        }

        // Event handlers

        private void CboTasks_SelectedIndexChanged(object? s, EventArgs e)
            => LoadSelectedTask();

        private void BtnAssignPerson_Click(object? s, EventArgs e)
        {
            if (_currentTask == null) return;
            if (cmbPersonAssign.SelectedItem is not Person p) return;

            var (ok, msg) = _taskCtrl.AssignPerson(_currentTask.TaskId, p.PersonId);
            if (!ok)
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            RefreshAssignedPersons();
        }

        private void BtnRemovePerson_Click(object? s, EventArgs e)
        {
            if (_currentTask == null) return;
            if (lstAssigned.SelectedItem is not Person p) return;

            _taskCtrl.RemovePerson(_currentTask.TaskId, p.PersonId);
            RefreshAssignedPersons();
        }

        private void BtnAssignTeam_Click(object? s, EventArgs e)
        {
            if (_currentTask == null) return;
            if (cmbTeamAssign.SelectedItem is not Team t) return;

            var (ok, msg) = _taskCtrl.AssignTeam(_currentTask.TaskId, t.TeamId);
            if (!ok)
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            RefreshAssignedTeams();
        }

        private void BtnRemoveTeam_Click(object? s, EventArgs e)
        {
            if (_currentTask == null) return;
            if (lstAssignedTeams.SelectedItem is not Team t) return;

            _taskCtrl.RemoveTeam(_currentTask.TaskId, t.TeamId);
            RefreshAssignedTeams();
        }

        private void BtnSaveChanges_Click(object? s, EventArgs e)
        {
            if (_currentTask == null) return;

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Title is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var updated = new ProjectTask
            {
                TaskId = _currentTask.TaskId,
                UserStoryId = _currentTask.UserStoryId,
                Title = txtTitle.Text.Trim(),
                Priority = (int)numPriority.Value,
                Difficulty = (int)numDifficulty.Value,
                CategoryLabels = txtLabels.Text.Trim(),
                PlannedTime = (float)numPlannedTime.Value,
                ActualTime = (float)numActualTime.Value,
                PlannedStartDate = chkPlannedStart.Checked ? dtpPlannedStart.Value : null,
                PlannedEndDate = chkPlannedEnd.Checked ? dtpPlannedEnd.Value : null,
                ActualStartDate = chkActualStart.Checked ? dtpActualStart.Value : null,
                ActualEndDate = chkActualEnd.Checked ? dtpActualEnd.Value : null,
                State = cmbState.Items.Count > 0
                                       ? (TaskState)cmbState.SelectedIndex
                                       : _currentTask.State
            };

            var (ok, msg) = _taskCtrl.UpdateTask(updated);
            if (!ok)
            {
                MessageBox.Show(msg, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Update local reference so cboTasks display stays correct
            _currentTask = _taskCtrl.GetById(updated.TaskId)!;

            // Refresh the task dropdown in case Title changed
            int selectedIdx = cboTasks.SelectedIndex;
            LoadTasks();
            if (selectedIdx < cboTasks.Items.Count)
                cboTasks.SelectedIndex = selectedIdx;

            lblStatus.Text = "✓ Saved!";
            lblStatus.ForeColor = Color.FromArgb(15, 110, 86);
        }

        // Helper builders

        private NumericUpDown MakeNum(int min, int max) => new NumericUpDown
        {
            Width = 90,
            Minimum = min,
            Maximum = max,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0)
        };

        private NumericUpDown MakeDecimalNum() => new NumericUpDown
        {
            Width = 120,
            Minimum = 0,
            Maximum = 9999,
            DecimalPlaces = 1,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0)
        };

        private DateTimePicker MakeDtp() => new DateTimePicker
        {
            Dock = DockStyle.Fill,
            Enabled = false,
            Format = DateTimePickerFormat.Short,
            Margin = new Padding(0, 2, 0, 2)
        };

        private CheckBox MakeChk(string text) => new CheckBox
        {
            Text = text,
            AutoSize = true,
            Width = 110,
            Margin = new Padding(0, 4, 8, 4)
        };

        private Label MakeLabel(string text) => new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = Color.FromArgb(80, 80, 76),
            Margin = new Padding(0, 0, 0, 4)
        };

        private Panel MakeSeparator() => new Panel
        {
            Dock = DockStyle.Fill,
            Height = 1,
            BackColor = Color.FromArgb(220, 218, 213),
            Margin = new Padding(0, 4, 0, 10)
        };
    }
}