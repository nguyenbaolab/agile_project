using System;
using System.Drawing;
using System.Windows.Forms;
using Agile_Project.Controllers;
using Agile_Project.Models;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Forms
{
    public class ManagePersonsDialog : Form
    {
        private readonly ProjectController _ctrl;
        private readonly Project _project;

        private ListBox lstProjectPersons = new();
        private ListBox lstAllPersons = new();
        private TextBox txtName = new();
        private TextBox txtRole = new();

        public ManagePersonsDialog(Project project, ProjectController ctrl)
        {
            _project = project;
            _ctrl = ctrl;

            Text = $"Manage Persons — {project.Name}";
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new Size(700, 550);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Font;
            Padding = new Padding(0);

            BuildUI();
            RefreshLists();
        }

        private void BuildUI()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill };

            // Tab "Add Person" — chỉ Admin
            if (PermissionService.CanDo("ManagePerson"))
                tabs.TabPages.Add(BuildAddTab());

            tabs.TabPages.Add(BuildProjectPersonsTab());
            tabs.TabPages.Add(BuildAllPersonsTab());
            Controls.Add(tabs);
        }

        // Tab 1: Add Person (Admin only)

        private TabPage BuildAddTab()
        {
            var tp = new TabPage("Add Person") { Padding = new Padding(12) };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.White,
                AutoSize = true,
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            layout.Controls.Add(MakeLabel("Name *"), 0, 0);

            txtName = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 10)
            };
            layout.Controls.Add(txtName, 0, 1);

            layout.Controls.Add(MakeLabel("Role"), 0, 2);

            txtRole = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 10)
            };
            layout.Controls.Add(txtRole, 0, 3);

            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            var btnAdd = new Button
            {
                Text = "Add & Link to Project",
                AutoSize = true,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(83, 74, 183),
                ForeColor = Color.White,
                Padding = new Padding(8, 0, 8, 0),
                Margin = new Padding(0, 6, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };
            btnAdd.Click += BtnAddPerson_Click;
            btnRow.Controls.Add(btnAdd);
            layout.Controls.Add(btnRow, 0, 4);

            tp.Controls.Add(layout);
            return tp;
        }

        // Tab 2: In This Project

        private TabPage BuildProjectPersonsTab()
        {
            var tp = new TabPage("In This Project") { Padding = new Padding(12) };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            lstProjectPersons = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 8, 0)
            };
            layout.Controls.Add(lstProjectPersons, 0, 0);

            var btnCol = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            // Remove from project — chỉ người có quyền AssignPerson (Admin/PO)
            if (PermissionService.CanDo("AssignPerson"))
            {
                var btnRemove = new Button
                {
                    Text = "Remove from project",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(160, 45, 45),
                    Margin = new Padding(0, 0, 0, 6),
                    FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
                };
                btnRemove.Click += BtnRemoveFromProject_Click;
                btnCol.Controls.Add(btnRemove);
            }

            layout.Controls.Add(btnCol, 1, 0);
            tp.Controls.Add(layout);
            return tp;
        }

        // Tab 3: All Persons

        private TabPage BuildAllPersonsTab()
        {
            var tp = new TabPage("All Persons") { Padding = new Padding(12) };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            lstAllPersons = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 8, 0)
            };
            layout.Controls.Add(lstAllPersons, 0, 0);

            var btnCol = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            // Link to project — Admin/PO
            if (PermissionService.CanDo("AssignPerson"))
            {
                var btnLink = new Button
                {
                    Text = "Link to project",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(15, 110, 86),
                    ForeColor = Color.White,
                    Margin = new Padding(0, 0, 0, 6),
                    FlatAppearance = { BorderSize = 0 }
                };
                btnLink.Click += BtnLinkToProject_Click;
                btnCol.Controls.Add(btnLink);
            }

            // Delete person — Admin only
            if (PermissionService.CanDo("ManagePerson"))
            {
                var btnDelete = new Button
                {
                    Text = "Delete person",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(160, 45, 45),
                    Margin = new Padding(0, 0, 0, 0),
                    FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
                };
                // Delete not in current controller API, placeholder
                btnCol.Controls.Add(btnDelete);
            }

            layout.Controls.Add(btnCol, 1, 0);
            tp.Controls.Add(layout);
            return tp;
        }

        // Data 

        private void RefreshLists()
        {
            lstProjectPersons.Items.Clear();
            foreach (var p in _ctrl.GetPersonsByProject(_project.ProjectId))
                lstProjectPersons.Items.Add(p);
            lstProjectPersons.DisplayMember = "Name";

            lstAllPersons.Items.Clear();
            foreach (var p in _ctrl.GetAllPersons())
                lstAllPersons.Items.Add(p);
            lstAllPersons.DisplayMember = "Name";
        }

        // Event handlers

        private void BtnAddPerson_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bool ok = _ctrl.AddPerson(txtName.Text.Trim(), txtRole.Text.Trim());
            if (ok)
            {
                var all = _ctrl.GetAllPersons();
                if (all.Count > 0)
                {
                    var newest = all[^1];
                    _ctrl.AddPersonToProject(_project.ProjectId, newest.PersonId);
                }
                txtName.Clear();
                txtRole.Clear();
                RefreshLists();
            }
        }

        private void BtnRemoveFromProject_Click(object? s, EventArgs e)
        {
            if (lstProjectPersons.SelectedItem is Person)
            {
                MessageBox.Show("Remove from project not yet wired to controller.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnLinkToProject_Click(object? s, EventArgs e)
        {
            if (lstAllPersons.SelectedItem is Person p)
            {
                _ctrl.AddPersonToProject(_project.ProjectId, p.PersonId);
                RefreshLists();
            }
        }

        // Helpers

        private Label MakeLabel(string text) => new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = Color.FromArgb(80, 80, 76),
            Margin = new Padding(0, 0, 0, 4)
        };
    }
}