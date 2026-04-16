using System;
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

        public AddEditUserStoryDialog(UserStory? existing, int projectId, UserStoryController ctrl)
        {
            _existing = existing;
            _projectId = projectId;
            _ctrl = ctrl;

            Text = existing == null ? "Add User Story" : "Edit User Story";

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Font;

            BuildUI();
            if (existing != null) FillData();
        }

        private void BuildUI()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.White,
                Padding = new Padding(16),
                AutoSize = true,
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Title section
            layout.Controls.Add(MakeLabel("Title *"), 0, 0);

            txtTitle = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.Controls.Add(txtTitle, 0, 1);

            // Description section
            layout.Controls.Add(MakeLabel("Description"), 0, 2);

            txtDesc = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 12),
                ScrollBars = ScrollBars.Vertical,
                WordWrap = true
            };
            layout.Controls.Add(txtDesc, 0, 3);

            // Priority section
            var priorityRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                Margin = new Padding(0),
                AutoSize = true
            };

            priorityRow.Controls.Add(MakeLabel("Priority"));

            numPriority = new NumericUpDown
            {
                Width = 100,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 4, 0, 0)
            };

            priorityRow.Controls.Add(numPriority);
            layout.Controls.Add(priorityRow, 0, 4);

            // Buttons section
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White,
                Margin = new Padding(0),
                Padding = new Padding(0, 0, 8, 8),
                AutoSize = false,
                Height = 45
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                AutoSize = true,
                MinimumSize = new Size(90, 35),
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 4, 0, 0),
                FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
            };

            var btnSave = new Button
            {
                Text = "Save",
                AutoSize = true,
                MinimumSize = new Size(90, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(83, 74, 183),
                ForeColor = Color.White,
                Margin = new Padding(6, 4, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };

            btnSave.Click += BtnSave_Click;

            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);
            layout.Controls.Add(btnPanel, 0, 5);

            Controls.Add(layout);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
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