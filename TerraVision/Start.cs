using System;

namespace TerraVision
{
    public partial class Start : UtilsForm
    {
        public Start()
        {
            InitializeComponent();
        }
        private void loginButton_Click(object sender, EventArgs e)
        {
            IFormHandler formHandler = new Login();
            formHandler.ShowForm();
            HideForm();
        }
        private void registerButton_Click(object sender, EventArgs e)
        {
            IFormHandler formHandler = new Register();
            formHandler.ShowForm();
            HideForm();
        }
    }
}
