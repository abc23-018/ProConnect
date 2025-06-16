using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome.Sharp;
using ProService.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ProService
{
    public partial class frmLogin : Form
    {
        public static class Session
        {
            public static string MembershipID { get; set; }
            public static string ProviderID { get; set; }
            public static string SelectedService { get; set; }
        }

        public static frmLogin instance;
        public frmLogin()
        {
            InitializeComponent();
            instance = this;
        }

        //public TextBox txtMembership;
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string membership = txtMembership.Text;
            string password = txtPassword.Text;

            // Use UserAuthentication class to validate login
            using (UserAuthentication auth = new UserAuthentication())
            {
                ProviderAuthentication authenticate = new ProviderAuthentication();
                if (auth.ValidateUser(membership, password))
                {
                    // Store the logged-in user's MembershipID
                    Session.MembershipID = membership;
                    // Login success: Show the dashboard form
                    Form frm = new frmDashboard();
                    frm.ShowDialog();

                    // Set the greeting label in frmCustomerProfile
                    //frmCustomerProfile.instance.lblGreeting.Text = membership;
                }
                else if (authenticate.ValidateUser(membership, password))
                {
                    // Store the logged-in user's ProviderID
                    Session.ProviderID = membership;
                    Form frm = new frmProviderDashboard();
                    frm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Invalid MembershipID or Password. Please try again.");
                }
            }


        }


        private void lnkSignup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form form = new frmSignup();
            form.ShowDialog();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                // If the checkbox is checked, hide the password
                txtPassword.UseSystemPasswordChar = true; // hide password characters
            }
            else
            {
                // If the checkbox is unchecked, show the password
                txtPassword.UseSystemPasswordChar = false; // show password characters
            }
        }

    }
}
