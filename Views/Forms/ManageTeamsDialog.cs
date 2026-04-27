using System;
using System.Drawing;
using System.Windows.Forms;
using Agile_Project.Controllers;
using Agile_Project.Models;
using Agile_Project.Models.Entities;

namespace Agile_Project.Views.Forms
{
    // Project-scoped team manager. Buttons follow PermissionService:
    //   ManageTeam (Admin)        -> + New Team / Delete Team / Save Team
    //   ManageTeamMember (Admin/PO) -> Add member / Remove member
    //   ViewTeam (everyone)       -> dialog still opens, but read-only.
    public class ManageTeamsDialog : Form
    {
        private readonly ITeamController _ctrl;
        private readonly Project _project;

        private ListBox lstTeams = new();
        private ListBox lstMembers = new();
        private ComboBox cboEligible = new();
        private TextBox txtName = new();
        private Label lblSelectedTeam = new();

        private Team? _selectedTeam;
        // True while creating a new team. Tells Save Team to call AddTeam instead of UpdateTeam.
        private bool _isCreating;

        public ManageTeamsDialog(Project project, ITeamController ctrl)
        {
            _project = project;
            _ctrl = ctrl;

            Text = $"Manage Teams — {project.Name}";
            Size = new Size(820, 560);
            MinimumSize = new Size(720, 500);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Font;
            Padding = new Padding(0);

            BuildUI();
            RefreshTeams();
        }

        private void BuildUI()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            root.Controls.Add(BuildLeftPanel(), 0, 0);
            root.Controls.Add(BuildRightPanel(), 1, 0);

            Controls.Add(root);
        }

        // Left: team list + create/delete

        private Control BuildLeftPanel()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 12, 0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layout.Controls.Add(MakeLabel("Teams"), 0, 0);

            lstTeams = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 8),
                DisplayMember = "Name"
            };
            lstTeams.SelectedIndexChanged += LstTeams_SelectedIndexChanged;
            layout.Controls.Add(lstTeams, 0, 1);

            // New / Delete team — Admin only (ManageTeam)
            if (PermissionService.CanDo("ManageTeam"))
            {
                var btnRow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    BackColor = Color.White,
                    Margin = new Padding(0)
                };

                var btnNew = new Button
                {
                    Text = "+ New Team",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(83, 74, 183),
                    ForeColor = Color.White,
                    Padding = new Padding(8, 0, 8, 0),
                    Margin = new Padding(0, 0, 6, 0),
                    FlatAppearance = { BorderSize = 0 }
                };
                btnNew.Click += BtnNewTeam_Click;
                btnRow.Controls.Add(btnNew);

                var btnDelete = new Button
                {
                    Text = "Delete Team",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(160, 45, 45),
                    Margin = new Padding(0, 0, 0, 0),
                    FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
                };
                btnDelete.Click += BtnDeleteTeam_Click;
                btnRow.Controls.Add(btnDelete);

                layout.Controls.Add(btnRow, 0, 2);
            }

            return layout;
        }

        // Right: selected team detail + members

        private Control BuildRightPanel()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.White
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            lblSelectedTeam = new Label
            {
                Text = "Select a team",
                AutoSize = true,
                ForeColor = Color.FromArgb(60, 60, 58),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 8)
            };
            layout.Controls.Add(lblSelectedTeam, 0, 0);

            layout.Controls.Add(MakeLabel("Name *"), 0, 1);
            txtName = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 8)
            };
            layout.Controls.Add(txtName, 0, 2);

            layout.Controls.Add(BuildMembersPanel(), 0, 3);
            layout.Controls.Add(BuildActionRow(), 0, 4);

            return layout;
        }

        private Control BuildMembersPanel()
        {
            var box = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                Margin = new Padding(0, 4, 0, 0)
            };
            box.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            box.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            box.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            box.Controls.Add(MakeLabel("Members"), 0, 0);

            lstMembers = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 8),
                DisplayMember = "Name"
            };
            box.Controls.Add(lstMembers, 0, 1);

            var addRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            cboEligible = new ComboBox
            {
                Width = 320,
                DropDownWidth = 360,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                DisplayMember = "Name",
                Margin = new Padding(0, 0, 6, 0)
            };
            addRow.Controls.Add(cboEligible);

            // Add / Remove member — Admin or ProductOwner (ManageTeamMember)
            if (PermissionService.CanDo("ManageTeamMember"))
            {
                var btnAdd = new Button
                {
                    Text = "Add member",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(15, 110, 86),
                    ForeColor = Color.White,
                    Padding = new Padding(8, 0, 8, 0),
                    Margin = new Padding(0, 0, 6, 0),
                    FlatAppearance = { BorderSize = 0 }
                };
                btnAdd.Click += BtnAddMember_Click;
                addRow.Controls.Add(btnAdd);

                var btnRemove = new Button
                {
                    Text = "Remove member",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(160, 45, 45),
                    Margin = new Padding(0, 0, 0, 0),
                    FlatAppearance = { BorderColor = Color.FromArgb(200, 198, 193) }
                };
                btnRemove.Click += BtnRemoveMember_Click;
                addRow.Controls.Add(btnRemove);
            }

            box.Controls.Add(addRow, 0, 2);
            return box;
        }

        private Control BuildActionRow()
        {
            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                BackColor = Color.White,
                Margin = new Padding(0, 8, 0, 0)
            };

            // Save Team — Admin only (ManageTeam)
            if (PermissionService.CanDo("ManageTeam"))
            {
                var btnSave = new Button
                {
                    Text = "Save Team",
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(83, 74, 183),
                    ForeColor = Color.White,
                    Padding = new Padding(8, 0, 8, 0),
                    Margin = new Padding(0),
                    FlatAppearance = { BorderSize = 0 }
                };
                btnSave.Click += BtnSaveTeam_Click;
                row.Controls.Add(btnSave);
            }

            return row;
        }

        // Data

        private void RefreshTeams()
        {
            int? keepId = _selectedTeam?.TeamId;

            lstTeams.Items.Clear();
            foreach (var t in _ctrl.GetByProject(_project.ProjectId))
                lstTeams.Items.Add(t);

            if (keepId.HasValue)
            {
                for (int i = 0; i < lstTeams.Items.Count; i++)
                {
                    if (lstTeams.Items[i] is Team t && t.TeamId == keepId.Value)
                    {
                        lstTeams.SelectedIndex = i;
                        return;
                    }
                }
            }

            ClearSelection();
        }

        private void RefreshMembers()
        {
            lstMembers.Items.Clear();
            cboEligible.Items.Clear();

            if (_selectedTeam == null) return;

            foreach (var p in _ctrl.GetMembers(_selectedTeam.TeamId))
                lstMembers.Items.Add(p);

            foreach (var p in _ctrl.GetEligiblePersons(_project.ProjectId, _selectedTeam.TeamId))
                cboEligible.Items.Add(p);
        }

        private void ClearSelection()
        {
            _selectedTeam = null;
            _isCreating = false;
            lblSelectedTeam.Text = "Select a team";
            txtName.Text = "";
            lstMembers.Items.Clear();
            cboEligible.Items.Clear();
        }

        // Event handlers

        private void LstTeams_SelectedIndexChanged(object? s, EventArgs e)
        {
            if (lstTeams.SelectedItem is Team t)
            {
                _isCreating = false;
                _selectedTeam = t;
                lblSelectedTeam.Text = t.Name;
                txtName.Text = t.Name;
                RefreshMembers();
            }
        }

        private void BtnNewTeam_Click(object? s, EventArgs e)
        {
            _isCreating = true;
            _selectedTeam = null;
            lstTeams.ClearSelected();
            lblSelectedTeam.Text = "New Team (enter name and click Save)";
            txtName.Text = "";
            lstMembers.Items.Clear();
            cboEligible.Items.Clear();
            txtName.Focus();
        }

        private void BtnDeleteTeam_Click(object? s, EventArgs e)
        {
            if (_selectedTeam == null)
            {
                MessageBox.Show("Please select a team first.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Delete team \"{_selectedTeam.Name}\"?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            var (ok, msg) = _ctrl.DeleteTeam(_selectedTeam.TeamId);
            if (!ok)
            {
                MessageBox.Show(msg, "Cannot delete",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ClearSelection();
            RefreshTeams();
        }

        private void BtnSaveTeam_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Team name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (_isCreating)
            {
                var (newId, addMsg) = _ctrl.AddTeam(_project.ProjectId, txtName.Text);
                if (newId < 0)
                {
                    MessageBox.Show(addMsg, "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _isCreating = false;
                _selectedTeam = new Team { TeamId = newId };
                RefreshTeams();
                return;
            }

            if (_selectedTeam == null)
            {
                MessageBox.Show("Please select a team first.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var (ok, msg) = _ctrl.UpdateTeam(_selectedTeam.TeamId, txtName.Text);
            if (!ok)
            {
                MessageBox.Show(msg, "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            RefreshTeams();
        }

        private void BtnAddMember_Click(object? s, EventArgs e)
        {
            if (_selectedTeam == null)
            {
                MessageBox.Show("Please select a team first.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (cboEligible.SelectedItem is not Person p)
            {
                MessageBox.Show("Please pick a person to add.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var (ok, msg) = _ctrl.AddMember(_selectedTeam.TeamId, p.PersonId);
            if (!ok)
                MessageBox.Show(msg, "Cannot add",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RefreshMembers();
        }

        private void BtnRemoveMember_Click(object? s, EventArgs e)
        {
            if (_selectedTeam == null) return;
            if (lstMembers.SelectedItem is not Person p)
            {
                MessageBox.Show("Please select a member first.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Remove \"{p.Name}\" from team \"{_selectedTeam.Name}\"?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            _ctrl.RemoveMember(_selectedTeam.TeamId, p.PersonId);
            RefreshMembers();
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
