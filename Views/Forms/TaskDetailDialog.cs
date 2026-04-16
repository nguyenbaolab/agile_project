using System;
using System.Drawing;
using System.Windows.Forms;
using Agile_Project.Controllers;
using Agile_Project.Models;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Forms
{
    public class TaskDetailDialog : Form
    {
        private readonly TaskController _taskCtrl;
        private readonly ProjectController _projectCtrl;
        private readonly UserStory _story;
        private readonly ProjectTask? _existing;

        private TextBox txtTitle = new();
        private NumericUpDown numPriority = new();
        private NumericUpDown numDifficulty = new();
        private TextBox txtLabels = new();
        private NumericUpDown numPlannedTime = new();
        private NumericUpDown numActualTime = new();
        private DateTimePicker dtpPlannedStart = new();
        private DateTimePicker dtpPlannedEnd = new();
        private DateTimePicker dtpActualStart = new();
        private DateTimePicker dtpActualEnd = new();
        private CheckBox chkPlannedStart = new();
        private CheckBox chkPlannedEnd = new();
        private CheckBox chkActualStart = new();
        private CheckBox chkActualEnd = new();
        private ComboBox cmbState = new();
        private ListBox lstPersons = new();
        private ComboBox cmbAssign = new();

        public TaskDetailDialog(UserStory story, ProjectTask? existing,
            ProjectController projectCtrl, TaskController taskCtrl)
        {
            _story = story;
            _existing = existing;
            _projectCtrl = projectCtrl;
            _taskCtrl = taskCtrl;

            Text = existing == null ? "Add Task" : "Task Detail";
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Font;
            Padding = new Padding(16, 12, 16, 12);

            BuildUI();
            LoadPersons();
            if (existing != null) FillData();
        }

        private void BuildUI()
        {
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            var layout = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                ColumnCount = 1,
                BackColor = Color.White,
                Padding = new Padding(0),
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Title
            layout.Controls.Add(MakeLabel("Title *"));
            txtTitle = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 10)
            };
            layout.Controls.Add(txtTitle);

            // Priority + Difficulty
            layout.Controls.Add(MakeLabel("Priority  /  Difficulty"));
            var rowPriDiff = new FlowLayoutPanel
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
            rowPriDiff.Controls.Add(numPriority);
            rowPriDiff.Controls.Add(numDifficulty);
            layout.Controls.Add(rowPriDiff);

            // Category Labels
            layout.Controls.Add(MakeLabel("Category labels (comma separated)"));
            txtLabels = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 10)
            };
            layout.Controls.Add(txtLabels);

            // Planned time + Actual time
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
            rowDates.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 150));

            int chkWidth = 110;

            chkPlannedStart = new CheckBox
            {
                Text = "Planned start",
                AutoSize = true,
                Width = chkWidth,
                Margin = new Padding(0, 4, 8, 4)
            };
            dtpPlannedStart = MakeDtp();
            chkPlannedStart.CheckedChanged += (s, e) => dtpPlannedStart.Enabled = chkPlannedStart.Checked;
            rowDates.Controls.Add(chkPlannedStart);
            rowDates.Controls.Add(dtpPlannedStart);

            chkPlannedEnd = new CheckBox
            {
                Text = "Planned end",
                AutoSize = true,
                Width = chkWidth,
                Margin = new Padding(0, 4, 8, 4)
            };
            dtpPlannedEnd = MakeDtp();
            chkPlannedEnd.CheckedChanged += (s, e) => dtpPlannedEnd.Enabled = chkPlannedEnd.Checked;
            rowDates.Controls.Add(chkPlannedEnd);
            rowDates.Controls.Add(dtpPlannedEnd);

            chkActualStart = new CheckBox
            {
                Text = "Actual start",
                AutoSize = true,
                Width = chkWidth,
                Margin = new Padding(0, 4, 8, 4)
            };
            dtpActualStart = MakeDtp();
            chkActualStart.CheckedChanged += (s, e) => dtpActualStart.Enabled = chkActualStart.Checked;
            rowDates.Controls.Add(chkActualStart);
            rowDates.Controls.Add(dtpActualStart);

            chkActualEnd = new CheckBox
            {
                Text = "Actual end",
                AutoSize = true,
                Width = chkWidth,
                Margin = new Padding(0, 4, 8, 4)
            };
            dtpActualEnd = MakeDtp();
            chkActualEnd.CheckedChanged += (s, e) => dtpActualEnd.Enabled = chkActualEnd.Checked;
            rowDates.Controls.Add(chkActualEnd);
            rowDates.Controls.Add(dtpActualEnd);

            layout.Controls.Add(rowDates);

            // State — chỉ hiện nếu InSprint VÀ có quyền ChangeTaskState
            if (_story.State == UserStoryState.InSprint && PermissionService.CanDo("ChangeTaskState"))
            {
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
                layout.Controls.Add(cmbState);
            }

            // Separator
            var sep = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 1,
                BackColor = Color.FromArgb(220, 218, 213),
                Margin = new Padding(0, 4, 0, 10)
            };
            layout.Controls.Add(sep);

            // Assigned persons — chỉ hiện nếu có quyền AssignPerson
            if (PermissionService.CanDo("AssignPerson"))
            {
                layout.Controls.Add(MakeLabel("Assigned persons"));

                var rowPersons = new TableLayoutPanel
                {
                    AutoSize = true,
                    ColumnCount = 2,
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 0, 10),
                    Dock = DockStyle.Fill
                };
                rowPersons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                rowPersons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); 

                lstPersons = new ListBox
                {
                    Dock = DockStyle.Fill,
                    Height = 100,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(0, 0, 6, 0)
                };
                rowPersons.Controls.Add(lstPersons, 0, 0);

                var btnCol = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    BackColor = Color.White
                };
                var btnRemovePerson = new Button
                {
                    Text = "Remove",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(160, 45, 45),
                    Margin = new Padding(0, 0, 0, 4),
                    FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
                };
                btnRemovePerson.Click += BtnRemovePerson_Click;
                btnCol.Controls.Add(btnRemovePerson);
                rowPersons.Controls.Add(btnCol, 1, 0);
                layout.Controls.Add(rowPersons);

                // Assign row
                var rowAssign = new TableLayoutPanel
                {
                    AutoSize = true,
                    ColumnCount = 2,
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 0, 10),
                    Dock = DockStyle.Fill 
                };
                rowAssign.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                rowAssign.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); 

                cmbAssign = new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(0, 0, 6, 0),
                };
                rowAssign.Controls.Add(cmbAssign, 0, 0);

                var btnAssign = new Button
                {
                    Text = "Assign",
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(83, 74, 183),
                    ForeColor = Color.White,
                    FlatAppearance = { BorderSize = 0 }
                };
                btnAssign.Click += BtnAssign_Click;
                rowAssign.Controls.Add(btnAssign, 1, 0);
                layout.Controls.Add(rowAssign);
            }

            // Save / Cancel
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                AutoSize = true,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 0, 0, 0),
                FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
            };

            var btnSave = new Button
            {
                Text = "Save",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(83, 74, 183),
                ForeColor = Color.White,
                Margin = new Padding(6, 0, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };
            btnSave.Click += BtnSave_Click;

            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);
            layout.Controls.Add(btnPanel);

            scroll.Controls.Add(layout);
            Controls.Add(scroll);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        // Data

        private void LoadPersons()
        {
            var projectPersons = _projectCtrl.GetPersonsByProject(_story.ProjectId);
            cmbAssign.DataSource = null;
            cmbAssign.DataSource = projectPersons;
            cmbAssign.DisplayMember = "Name";
            RefreshAssignedList();
        }

        private void RefreshAssignedList()
        {
            if (_existing == null) return;
            lstPersons.Items.Clear();
            var report = _taskCtrl.GetTaskReport(_existing.TaskId);
            foreach (var p in _projectCtrl.GetPersonsByProject(_story.ProjectId))
            {
                // Full implementation needs TaskController.GetAssignedPersons()
            }
        }

        private void FillData()
        {
            txtTitle.Text = _existing!.Title;
            numPriority.Value = _existing.Priority;
            numDifficulty.Value = _existing.Difficulty;
            txtLabels.Text = _existing.CategoryLabels;
            numPlannedTime.Value = (decimal)_existing.PlannedTime;
            numActualTime.Value = (decimal)_existing.ActualTime;

            if (_existing.PlannedStartDate.HasValue) { chkPlannedStart.Checked = true; dtpPlannedStart.Value = _existing.PlannedStartDate.Value; }
            if (_existing.PlannedEndDate.HasValue) { chkPlannedEnd.Checked = true; dtpPlannedEnd.Value = _existing.PlannedEndDate.Value; }
            if (_existing.ActualStartDate.HasValue) { chkActualStart.Checked = true; dtpActualStart.Value = _existing.ActualStartDate.Value; }
            if (_existing.ActualEndDate.HasValue) { chkActualEnd.Checked = true; dtpActualEnd.Value = _existing.ActualEndDate.Value; }

            if (cmbState.Items.Count > 0)
                cmbState.SelectedIndex = (int)_existing.State;
        }

        // Event handlers

        private void BtnAssign_Click(object? s, EventArgs e)
        {
            if (_existing == null)
            {
                MessageBox.Show("Save the task first before assigning persons.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (cmbAssign.SelectedItem is Person p)
            {
                var (ok, msg) = _taskCtrl.AssignPerson(_existing.TaskId, p.PersonId);
                if (!ok) MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else RefreshAssignedList();
            }
        }

        private void BtnRemovePerson_Click(object? s, EventArgs e)
        {
            if (_existing == null || lstPersons.SelectedItem == null) return;
            if (lstPersons.SelectedItem is Person p)
            {
                _taskCtrl.RemovePerson(_existing.TaskId, p.PersonId);
                RefreshAssignedList();
            }
        }

        private void BtnSave_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Title is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_existing == null)
            {
                bool ok = _taskCtrl.AddTask(_story.UserStoryId, txtTitle.Text.Trim(), (int)numPriority.Value);
                if (!ok)
                {
                    MessageBox.Show("Could not add task. Story may be in Done state.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                _taskCtrl.UpdatePriority(_existing.TaskId, (int)numPriority.Value);

                if (cmbState.Items.Count > 0)
                {
                    var newState = (TaskState)cmbState.SelectedIndex;
                    if (newState != _existing.State)
                    {
                        var (ok, msg) = _taskCtrl.ChangeState(_existing.TaskId, newState);
                        if (!ok) MessageBox.Show(msg, "State Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        // Helpers

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

        private Label MakeLabel(string text) => new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = Color.FromArgb(80, 80, 76),
            Margin = new Padding(0, 0, 0, 4)
        };
    }
}