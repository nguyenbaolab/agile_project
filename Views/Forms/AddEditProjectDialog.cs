using System;
using System.Drawing;
using System.Windows.Forms;
using Agile_Project.Controllers;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Forms
{
    public class AddEditProjectDialog : Form
    {
        private readonly ProjectController _ctrl;
        private readonly Project? _existing;

        private TextBox txtName = new();
        private TextBox txtDesc = new();

        public AddEditProjectDialog(Project? existing, ProjectController ctrl)
        {
            _existing = existing;
            _ctrl = ctrl;

            Text = existing == null ? "New Project" : "Edit Project";
            Size = new Size(430, 350);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            Padding = new Padding(16);

            BuildUI();
            if (existing != null) FillData();
        }

        private void BuildUI()
        {
            // Dung TableLayoutPanel de cac control tu xep thang hang, khong bi dinh
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5, // lblName, txtName, lblDesc, txtDesc, buttons
                BackColor = Color.White,
                Padding = new Padding(0)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // label Name
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));    // txtName
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // label Desc
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));    // txtDesc
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));    // buttons

            // Label: Project name
            var lblName = new Label
            {
                Text = "Project name *",
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 76),
                Margin = new Padding(0, 0, 0, 4)
            };
            layout.Controls.Add(lblName, 0, 0);

            // TextBox: Name
            txtName = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.Controls.Add(txtName, 0, 1);

            // Label: Description
            var lblDesc = new Label
            {
                Text = "Description",
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 76),
                Margin = new Padding(0, 0, 0, 4)
            };
            layout.Controls.Add(lblDesc, 0, 2);

            // TextBox: Description
            txtDesc = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.Controls.Add(txtDesc, 0, 3);

            // Button row
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
                Height = 50,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 4, 0, 0),
                FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
            };

            var btnSave = new Button
            {
                Text = "Save",
                Width = 90,
                Height = 50,
                DialogResult = DialogResult.None,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(83, 74, 183),
                ForeColor = Color.White,
                Margin = new Padding(6, 4, 0, 0),
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