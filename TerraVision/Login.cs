using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TerraVision
{
    public partial class Login : UtilsForm
    {
        public Login()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var username = usernameTextBox.Text;
            var password = passwordTextBox.Text;
            var hashedPassword = HashPassword(password);
            var users = LoadUsers();

            if (!UserExists(users, username))
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("User does not exist.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var user = users.Find(u => u.Username == username);
            if (user.Password != hashedPassword)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Incorrect password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            var loggedInUserPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "loggedInUser.json");
            var serializedData = JsonConvert.SerializeObject(user);
            File.WriteAllText(loggedInUserPath, serializedData);
            
            IFormHandler formHandler = new Map(user);
            formHandler.ShowForm();
            HideForm();
            Cursor.Current = Cursors.Default;
        }

        private void SwitchToRegisterButton_Click(object sender, EventArgs e)
        {
            HideForm();
            IFormHandler formHandler = new Register();
            formHandler.ShowForm();
        }
    }
}
