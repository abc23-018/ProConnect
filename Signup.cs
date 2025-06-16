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

namespace ProService
{
    public partial class frmSignup : Form
    {
        public frmSignup()
        {
            InitializeComponent();
        }

        private string GenerateMembershipID(string firstName, SqlConnection con)
        {
            int count = 1;
            string membershipID;

            do
            {
                // Use the full first name and append a 2-digit number
                membershipID = firstName.ToUpper() + count.ToString("D2");

                // Query the database to check if the MembershipID already exists
                string checkQuery = "SELECT COUNT(*) FROM SignUp WHERE MembershipID = @MembershipID";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@MembershipID", membershipID);

                int existingRecords = (int)checkCmd.ExecuteScalar();  // Check if the ID already exists

                if (existingRecords == 0)
                {
                    break;  // If no record exists with this ID, it's unique
                }

                count++;  // Increment count to try a new ID
            }
            while (true);

            return membershipID;
        }

        //Data dbConn = new Data();
        private void btnSignup_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            string firstName = txtFirstName.Text;
            using (UserSignup signup = new UserSignup())
            {
                string membershipID = GenerateMembershipID(firstName, signup.connection);  

                if (!string.IsNullOrEmpty(membershipID))
                {
                    signup.AddUser(membershipID, txtFirstName.Text, txtSurname.Text, cmbGender.SelectedItem.ToString(),
                                       txtIDno.Text, dtbDOB.Value, txtPhone.Text, txtEmail.Text, txtPlotno.Text, txtLocation.Text,
                                       txtPassword.Text);

                    MessageBox.Show("Account created successfully. Your membershipID is " + membershipID);
                }
                else
                {
                    MessageBox.Show("Error: MembershipID could not be generated.");
                }
            }
             
        }

        private bool ValidateInputs()
        {
            // First Name
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || !IsAlphabetic(txtFirstName.Text))
            {
                MessageBox.Show("Please enter a valid first name (alphabetic characters only).");
                return false;
            }

            // Surname
            if (string.IsNullOrWhiteSpace(txtSurname.Text) || !IsAlphabetic(txtSurname.Text))
            {
                MessageBox.Show("Please enter a valid surname (alphabetic characters only).");
                return false;
            }

            // Gender
            if (cmbGender.SelectedItem == null)
            {
                MessageBox.Show("Please select gender.");
                return false;
            }

            // ID Number
            if (string.IsNullOrWhiteSpace(txtIDno.Text) || !IsNumeric(txtIDno.Text) || txtIDno.Text.Length != 9)
            {
                MessageBox.Show("Please enter a valid 9-digit ID number.");
                return false;
            }

            // Date of Birth
            if (dtbDOB.Value.Date >= DateTime.Now.Date.AddYears(-18))
            {
                MessageBox.Show("You must be at least 18 years old.");
                return false;
            }

            // Phone Number
            if (string.IsNullOrWhiteSpace(txtPhone.Text) || !IsNumeric(txtPhone.Text) || txtPhone.Text.Length != 8)
            {
                MessageBox.Show("Please enter a valid 8-digit phone number.");
                return false;
            }

            // Email
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.");
                return false;
            }

            // Address
            if (string.IsNullOrWhiteSpace(txtPlotno.Text) || string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show("Please provide your full address.");
                return false;
            }

            // Password
            if (string.IsNullOrWhiteSpace(txtPassword.Text) || txtPassword.Text.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.");
                return false;
            }

            return true;
        }

        //Helper Methods: IsAlphabetic, IsNumeric, and IsValidEmail are utility functions to check input formats.
        private bool IsAlphabetic(string text)
        {
            return text.All(char.IsLetter);
        }

        private bool IsNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form frm = new frmProviderSignup();
            frm.ShowDialog();
        }

        private void lnkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form frm = new frmLogin();
            frm.ShowDialog();
        }
    }
}
