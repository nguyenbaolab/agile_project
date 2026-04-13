using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Agile_Project.Controllers;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Forms
{
    public class AddEditUserStoryDialog : Form
    {
        private readonly UserStoryController _ctrl;
        private readonly UserStory? _existing;
        private readonly int _projectId;

        private TextBox txtTitle = new();
        private TextBox txtDesc = new();
        private NumericUpDown numPriority = new();
        private CheckedListBox clbDeps = new();
        private List<UserStory> _allStories = new();

        public AddEditUserStoryDialog(UserStory? existing, int projectId, UserStoryController ctrl)
        {
            _existing = existing;
            _projectId = projectId;
            _ctrl = ctrl;

            Text = existing == null ? "Add User Story" : "Edit User Story";
            Size = new Size(440, 460);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            Padding = new Padding(16);

            BuildUI();
            LoadDependencies();
            if (existing != null) FillData();
        }

        private void BuildUI()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7, // lblTitle, txtTitle, lblDesc, txtDesc, lblPriority+num, lblDeps, clbDeps, buttons
                BackColor = Color.White,
                Padding = new Padding(0)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // label Title
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));   // txtTitle
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // label Desc
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));   // txtDesc
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));   // label Priority + numPriority
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // label Deps
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));  // clbDeps
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // buttons

            // --- Label: Title ---
            layout.Controls.Add(MakeLabel("Title *"), 0, 0);

            // --- TextBox: Title ---
            txtTitle = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.Controls.Add(txtTitle, 0, 1);

            // --- Label: Description ---
            layout.Controls.Add(MakeLabel("Description"), 0, 2);

            // --- TextBox: Description ---
            txtDesc = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.Controls.Add(txtDesc, 0, 3);

            // --- Priority row (label + numeric side by side) ---
            var priorityRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            priorityRow.Controls.Add(MakeLabel("Priority"));
            numPriority = new NumericUpDown
            {
                Width = 80,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 4, 0, 0)
            };
            priorityRow.Controls.Add(numPriority);
            layout.Controls.Add(priorityRow, 0, 4);

            // --- Label: Dependencies ---
            layout.Controls.Add(MakeLabel("Depends on (other stories that must be InSprint/Done first):"), 0, 5);

            // --- CheckedListBox: Dependencies ---
            clbDeps = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true,
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.Controls.Add(clbDeps, 0, 6);

            // --- Button row ---
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Width = 90,
                Height = 30,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 4, 0, 0),
                FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
            };

            var btnSave = new Button
            {
                Text = "Save",
                Width = 90,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(83, 74, 183),
                ForeColor = Color.White,
                Margin = new Padding(6, 4, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };
            btnSave.Click += BtnSave_Click;

            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);
            layout.Controls.Add(btnPanel, 0, 7);

            Controls.Add(layout);
            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void LoadDependencies()
        {
            _allStories = _ctrl.GetByProject(_projectId);
            clbDeps.Items.Clear();
            foreach (var s in _allStories)
            {
                if (_existing != null && s.UserStoryId == _existing.UserStoryId) continue;
                clbDeps.Items.Add(s);
            }
            clbDeps.DisplayMember = "Title";
        }

        private void FillData()
        {
            txtTitle.Text = _existing!.Title;
            txtDesc.Text = _existing.Description;
            numPriority.Value = _existing.Priority;
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
                _ctrl.AddUserStory(_projectId, txtTitle.Text.Trim(),
                    txtDesc.Text.Trim(), (int)numPriority.Value);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private Label MakeLabel(string text) => new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = Color.FromArgb(80, 80, 76),
            Margin = new Padding(0, 0, 0, 4)
        };
    }
}