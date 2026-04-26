using System;
using System.Drawing;
using System.Windows.Forms;
using Agile_Project.Controllers;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Forms
{
    public class AddEditUserStoryDialog : Form
    {
        private readonly IUserStoryController _ctrl;
        private readonly UserStory? _existing;
        private readonly int _projectId;

        private TextBox txtTitle = new();
        private TextBox txtDesc = new();
        private ComboBox cboPriority = new();

        public AddEditUserStoryDialog(UserStory? existing, int projectId, IUserStoryController ctrl)
        {
            _existing = existing;
            _projectId = projectId;
            _ctrl = ctrl;

            Text = existing == null ? "Add User Story" : "Edit User Story";

            ClientSize = new Size(480, 460);
            MinimumSize = new Size(440, 420);

            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10f);
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
                Padding = new Padding(20, 16, 20, 16),
                AutoSize = false,
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Title
            layout.Controls.Add(MakeLabel("Title *"), 0, 0);

            txtTitle = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.Controls.Add(txtTitle, 0, 1);

            // Description
            layout.Controls.Add(MakeLabel("Description"), 0, 2);

            txtDesc = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 12),
                ScrollBars = ScrollBars.None,
                WordWrap = true,
                MaxLength = 300,
                MinimumSize = new Size(0, 130)
            };
            layout.Controls.Add(txtDesc, 0, 3);

            // Priority
            var priorityRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                Margin = new Padding(0),
                AutoSize = true
            };

            priorityRow.Controls.Add(MakeLabel("Priority *"));

            cboPriority = new ComboBox
            {
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 0, 0)
            };
            cboPriority.Items.Add("1 - High");
            cboPriority.Items.Add("2 - Medium");
            cboPriority.Items.Add("3 - Low");

            priorityRow.Controls.Add(cboPriority);
            layout.Controls.Add(priorityRow, 0, 4);

            // Buttons
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White,
                Margin = new Padding(0, 8, 0, 0),
                Padding = new Padding(0, 4, 4, 0),
                AutoSize = false,
                Height = 50
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                AutoSize = false,
                Size = new Size(100, 36),
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8, 0, 0, 0),
                FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
            };

            var btnSave = new Button
            {
                Text = "Save",
                AutoSize = false,
                Size = new Size(100, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(83, 74, 183),
                ForeColor = Color.White,
                Margin = new Padding(8, 0, 0, 0),
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
            cboPriority.SelectedIndex = _existing.Priority switch
            {
                1 => 0,
                2 => 1,
                3 => 2,
                _ => -1
            };
        }

        private void BtnSave_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Title is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboPriority.SelectedIndex < 0)
            {
                MessageBox.Show("Priority is required. Please choose High, Medium or Low.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int priority = cboPriority.SelectedIndex + 1;
            string title = txtTitle.Text.Trim();
            string desc = txtDesc.Text.Trim();

            bool ok;
            if (_existing == null)
            {
                ok = _ctrl.AddUserStory(_projectId, title, desc, priority);
            }
            else
            {
                ok = _ctrl.UpdateUserStory(_existing.UserStoryId, title, desc, priority);
            }

            if (!ok)
            {
                MessageBox.Show("Could not save the user story. Check the inputs and try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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