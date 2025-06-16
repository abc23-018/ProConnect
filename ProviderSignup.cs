using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProService
{
    public partial class frmProviderSignup : Form
    {
        public frmProviderSignup()
        {
            InitializeComponent();
        }

        private void lnkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form frm = new frmLogin();
            frm.ShowDialog();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form frm = new frmSignup();
            frm.ShowDialog();
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private string GenerateProviderID(string name, SqlConnection con)
        {
            int count = 1;
            string BusinessName;

            do
            {
                // Use the full first name and append a 2-digit number
                BusinessName = name.ToUpper() + count.ToString("D2");

                // Query the database to check if the MembershipID already exists
                string checkQuery = "SELECT COUNT(*) FROM ProviderSignUp WHERE BusinessName = @BusinessName";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@BusinessName", BusinessName);

                int existingRecords = (int)checkCmd.ExecuteScalar();  // Check if the ID already exists

                if (existingRecords == 0)
                {
                    break;  // If no record exists with this ID, it's unique
                }
                else
                {
                    count++;  // Increment count to try a new ID
                }
                
            }
            while (true);

            return BusinessName;
        }
        
        private void btnSignup_Click(object sender, EventArgs e)
        {
            string businessname = txtName.Text.Split(' ')[0]; // Take only the first part before any space

            using (ServiceSignup signup = new ServiceSignup())
            {
                if (!ValidateInputs())
                    return;

                string ProviderID = GenerateProviderID(businessname, signup.connection);

                if (!string.IsNullOrEmpty(ProviderID))
                {
                    signup.AddUser(ProviderID, txtFirstName.Text, txtSurname.Text, cmbGender.SelectedItem.ToString(),
                                   txtIDno.Text, dtpRegdate.Value, txtPhone.Text, txtEmail.Text, cmbService.SelectedItem.ToString(), txtRegno.Text,
                                   txtPassword.Text, txtName.Text, txtDescription.Text, txtLocation.Text);

                    MessageBox.Show("Account created");
                }
                else
                {
                    MessageBox.Show("Error: ProviderID could not be generated.");
                }
            }


            // Loop through all controls in the form
            foreach (Control ctrl in this.Controls)
            {
                // Clear TextBoxes
                if (ctrl is TextBox textBox)
                {
                    textBox.Clear();
                }

                // Clear ComboBoxes
                else if (ctrl is ComboBox comboBox)
                {
                    comboBox.SelectedIndex = -1; // Reset selected index
                    comboBox.Text = "";          // Clear the text
                }

                // Clear DateTimePickers
                else if (ctrl is DateTimePicker dateTimePicker)
                {
                    dateTimePicker.Value = DateTime.Now; // Reset to current date
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

            // Service
            if (cmbService.SelectedItem == null)
            {
                MessageBox.Show("Please select service.");
                return false;
            }

            // Registration Number
            //Regex is Regular Expressions
            if (string.IsNullOrWhiteSpace(txtRegno.Text) || !Regex.IsMatch(txtRegno.Text, @"^BW-\d{8}$"))
            {
                MessageBox.Show("Please enter a valid registration number in the format 'BW-XXXXXXXX' (BW, hyphen, and 8 digits).");
                return false;
            }

            // Business name
            if (string.IsNullOrWhiteSpace(txtName.Text) || !IsValidBusinessName(txtName.Text))
            {
                MessageBox.Show("Please enter a valid Business name.");
                return false;
            }

            // Date of registration
            if (dtpRegdate.Value.Date >= DateTime.Now)
            {
                MessageBox.Show("Registration date cannot be any date later than today.");
                return false;
            }

            // Password
            if (string.IsNullOrWhiteSpace(txtPassword.Text) || txtPassword.Text.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.");
                return false;
            }

            // Description
            if (string.IsNullOrWhiteSpace(txtDescription.Text) || txtDescription.Text.Length > 500 || txtDescription.Text.Length < 20)
            {
                MessageBox.Show("Description should be a minimum of 20 characters and maximum of 500 characters.");
                return false;
            }

            // Location
            if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show("Please eneter a place in Greater Gaborone");
                return false;
            }

            return true;
        }

        //Helper Methods: IsAlphabetic, IsNumeric, and IsValidEmail are utility functions to check input formats.
        private bool IsAlphabetic(string text)
        {
            return text.All(char.IsLetter);
        }

        private bool IsValidBusinessName(string text)
        {
            // Allow only alphabetic characters and optional spaces
            return text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
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
    }
}
