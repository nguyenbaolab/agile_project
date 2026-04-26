using System;
using System.Drawing;
using System.Windows.Forms;
using Agile_Project.Controllers;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Forms
{
    public class AddEditProjectDialog : Form
    {
        private readonly IProjectController _ctrl;
        private readonly Project? _existing;

        private TextBox txtName = new();
        private TextBox txtDesc = new();

        public AddEditProjectDialog(Project? existing, IProjectController ctrl)
        {
            _existing = existing;
            _ctrl = ctrl;

            Text = existing == null ? "New Project" : "Edit Project";
            AutoSize = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10f);
            Padding = new Padding(16);
            ClientSize = new Size(460, 360);
            MinimumSize = new Size(460, 360);

            BuildUI();
            if (existing != null) FillData();
        }

        private void BuildUI()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5, // lblName, txtName, lblDesc, txtDesc, buttons
                BackColor = Color.White,
                Padding = new Padding(0),
                AutoSize = false
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            // Project name
            var lblName = new Label
            {
                Text = "Project name *",
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 76),
                Margin = new Padding(0, 0, 0, 4)
            };
            layout.Controls.Add(lblName, 0, 0);

            txtName = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.Controls.Add(txtName, 0, 1);

            // Description
            var lblDesc = new Label
            {
                Text = "Description",
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 76),
                Margin = new Padding(0, 0, 0, 4)
            };
            layout.Controls.Add(lblDesc, 0, 2);

            txtDesc = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                MaxLength = 500,
                BorderStyle = BorderStyle.FixedSingle,
                MinimumSize = new Size(0, 110),
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.Controls.Add(txtDesc, 0, 3);

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White,
                Margin = new Padding(0),
                Padding = new Padding(0, 6, 0, 0)
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                AutoSize = false,
                Size = new Size(90, 34),
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8, 0, 0, 0),
                FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
            };

            var btnSave = new Button
            {
                Text = "Save",
                AutoSize = false,
                Size = new Size(90, 34),
                DialogResult = DialogResult.None,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(83, 74, 183),
                ForeColor = Color.White,
                Margin = new Padding(8, 0, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };
            btnSave.Click += BtnSave_Click;

            // FlowDirection.RightToLeft nen them Cancel truoc, roi Save
            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);

            layout.Controls.Add(btnPanel, 0, 4);

            Controls.Add(layout);
            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void FillData()
        {
            txtName.Text = _existing!.Name;
            txtDesc.Text = _existing.Description;
        }

        private void BtnSave_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Project name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_existing == null)
            {
                _ctrl.AddProject(txtName.Text.Trim(), txtDesc.Text.Trim());
            }
            else
            {
                _existing.Name = txtName.Text.Trim();
                _existing.Description = txtDesc.Text.Trim();
                _ctrl.UpdateProject(_existing);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}