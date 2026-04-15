using System;
using System.Drawing;
using System.Windows.Forms;
using Agile_Project.Controllers;

namespace Agile_Project.Views.Forms
{
    public class LoginForm : Form
    {
        private readonly ProjectController _ctrl;
        private TextBox txtUsername = new();
        private TextBox txtPassword = new();
        private Label lblError = new();

        public LoginForm(ProjectController ctrl)
        {
            _ctrl = ctrl;

            Text = "Login — Agile Project Manager";
            Size = new Size(360, 260);
            MinimumSize = new Size(320, 240);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);

            BuildUI();
        }

        private void BuildUI()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.White,
                Padding = new Padding(24, 20, 24, 16)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // title
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // lbl username
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // txt username
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // lbl password
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // txt password
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // btn + error

            var lblTitle = new Label
            {
                Text = "Agile Project Manager",
                Dock = DockStyle.Fill,
                AutoSize = false,
                Height = 36,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(83, 74, 183),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 0, 8)
            };
            layout.Controls.Add(lblTitle, 0, 0);

            layout.Controls.Add(MakeLabel("Username"), 0, 1);

            txtUsername = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 2, 0, 8)
            };
            layout.Controls.Add(txtUsername, 0, 2);

            layout.Controls.Add(MakeLabel("Password"), 0, 3);

            txtPassword = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●',
                Margin = new Padding(0, 2, 0, 8)
            };
            layout.Controls.Add(txtPassword, 0, 4);

            // Bottom row: error label + login button
            var bottomPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            lblError = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(160, 45, 45),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = ""
            };
            bottomPanel.Controls.Add(lblError, 0, 0);

            var btnLogin = new Button
            {
                Text = "Login",
                AutoSize = true,                     
                MinimumSize = new Size(90, 32),         
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(83, 74, 183),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 },
                Margin = new Padding(8, 4, 0, 0)
            };
            btnLogin.Click += BtnLogin_Click;
            bottomPanel.Controls.Add(btnLogin, 1, 0);

            layout.Controls.Add(bottomPanel, 0, 5);

            Controls.Add(layout);
            AcceptButton = btnLogin;
            txtUsername.Focus();
        }

        private void BtnLogin_Click(object? s, EventArgs e)
        {
            lblError.Text = "";
            var (success, message) = new ProjectController().Login(
                txtUsername.Text.Trim(), txtPassword.Text);

            if (success)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblError.Text = message;
            }
        }

        private Label MakeLabel(string text) => new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = Color.FromArgb(80, 80, 76),
            Margin = new Padding(0, 0, 0, 2)
        };
    }
}