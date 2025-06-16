using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProService
{
    public static class Session
    {
        public static string MembershipID { get; set; }
        public static string ProviderID { get; set; }
        public static string SelectedService { get; set; }
        public static string SelectedProvider { get; set; }
        public static string BookingID { get; set; }
    }

    public abstract class Data : IDisposable
    {
        public string conString = "Data Source=Megan;" +
            "Initial Catalog=ProConnect;" +
            "Integrated Security=True;" ;  

        public SqlConnection connection;

        // Constructor initializes the connection
        protected Data()
        {
            connection = new SqlConnection(conString);
            connection.Open();
        }

        // Method to check if the connection is open
        protected void EnsureConnectionOpen()
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
        }

        // Implement IDisposable to ensure proper cleanup
        public void Dispose()
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose(); // Dispose of the connection object
            }
        }
    }


    public class UserAuthentication : Data
    {
        public bool ValidateUser(string membershipID, string password)
        {
            EnsureConnectionOpen();

            string query = "SELECT COUNT(1) FROM SignUp WHERE MembershipID = @MembershipID AND Password = @Password";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@MembershipID", membershipID);
                cmd.Parameters.AddWithValue("@Password", password);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 1)
                {
                    // Update session
                    Session.MembershipID = membershipID;
                    return true;
                }

                return false;
            }
        }
    }

    public class ProviderAuthentication : Data
    {
        public bool ValidateUser(string providerID, string password)
        {
            EnsureConnectionOpen();

            string query = "SELECT COUNT(1) FROM ProviderSignUp WHERE ProviderID = @ProviderID AND Password = @Password";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@ProviderID", providerID);
                cmd.Parameters.AddWithValue("@Password", password);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 1)
                {
                    // Update session
                    Session.ProviderID = providerID;
                    return true;
                }

                return false;
            }
        }
    }

    public class UserSignup : Data
    {
        // Method to insert a new user into the SignUp table
        public void AddUser(string membershipID, string firstName, string surname, string gender, string idNo, DateTime dob,
                            string phone, string email, string plotNo, string location, string password)
        {
            EnsureConnectionOpen();

            string query = "INSERT INTO SignUp (MembershipID, FirstName, Surname, Gender, IDno, DateOfBirth, PhoneNumber, " +
                           "EmailAddress, PlotNo, Location, Password) " +
                           "VALUES(@MembershipID, @FirstName, @Surname, @Gender, @IDno, @DateOfBirth, @PhoneNumber, @EmailAddress, @PlotNo, @Location, @Password)";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                // Add parameters
                cmd.Parameters.AddWithValue("@MembershipID", membershipID);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@Surname", surname);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@IDno", idNo);
                cmd.Parameters.AddWithValue("@DateOfBirth", dob);
                cmd.Parameters.AddWithValue("@PhoneNumber", phone);
                cmd.Parameters.AddWithValue("@EmailAddress", email);
                cmd.Parameters.AddWithValue("@PlotNo", plotNo);
                cmd.Parameters.AddWithValue("@Location", location);
                cmd.Parameters.AddWithValue("@Password", password);

                // Execute the query
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class UserLoader : Data
    {
        // Method to load user data into textboxes based on MembershipID
        public void LoadUserData(string membershipID, TextBox txtFirstName, TextBox txtSurname, ComboBox cmbGender,
                                 TextBox txtIDNo, DateTimePicker dtpDOB, TextBox txtPhone, TextBox txtEmail,
                                 TextBox txtPlotNo, TextBox txtLocation, TextBox txtPassword)
        {
            EnsureConnectionOpen();
            string currentMembershipID = Session.MembershipID;
            string query = "SELECT FirstName, Surname, Gender, IDno, DateOfBirth, PhoneNumber, EmailAddress, PlotNo, Location, Password " +
                           "FROM SignUp WHERE MembershipID = '" + currentMembershipID + "'";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@MembershipID", membershipID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Load data into textboxes
                        txtFirstName.Text = reader["FirstName"].ToString();
                        txtSurname.Text = reader["Surname"].ToString();
                        cmbGender.SelectedItem = reader["Gender"].ToString();
                        txtIDNo.Text = reader["IDno"].ToString();
                        txtPhone.Text = reader["PhoneNumber"].ToString();
                        txtEmail.Text = reader["EmailAddress"].ToString();
                        txtPlotNo.Text = reader["PlotNo"].ToString();
                        txtLocation.Text = reader["Location"].ToString();
                        txtPassword.Text = reader["Password"].ToString();

                        // Load data into DateTimePicker
                        if (DateTime.TryParse(reader["DateOfBirth"].ToString(), out DateTime dob))
                        {
                            dtpDOB.Value = dob;
                        }
                        else
                        {
                            dtpDOB.Value = DateTime.Now; // Set a default value if parsing fails
                        }
                    }
                    else
                    {
                        throw new Exception("No user found with the provided MembershipID.");
                    }
                }
            }
        }
    }

    public class UpdateUserSignup : Data
    {
        // Method to update user information in the SignUp table
        public void UpdateUser(string membershipID, string firstName, string surname, string gender, string idNo, DateTime dob,
                            string phone, string email, string plotno,string location, string password)
        {
            EnsureConnectionOpen();
            string currentMembershipID = Session.MembershipID;
            string query = "UPDATE SignUp SET " +
                   "FirstName = @FirstName, " +
                   "Surname = @Surname, " +
                   "Gender = @Gender, " +
                   "IDno = @IDno, " +
                   "DateOfBirth = @DOB,"+
                   "PhoneNumber = @PhoneNumber, " +
                   "EmailAddress = @EmailAddress, " +
                   "PlotNo = @PlotNo, " +
                   "Location = @Location, " +
                   "Password = @Password "+
                   "WHERE MembershipID = '" + currentMembershipID + "'";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                // Add parameters
                cmd.Parameters.AddWithValue("@MembershipID", membershipID);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@Surname", surname);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@IDno", idNo);
                cmd.Parameters.AddWithValue("@DOB", dob.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@PhoneNumber", phone);
                cmd.Parameters.AddWithValue("@EmailAddress", email);
                cmd.Parameters.AddWithValue("@PlotNo", plotno);
                cmd.Parameters.AddWithValue("@Location", location);
                cmd.Parameters.AddWithValue("@Password", password);
                // Execute the query
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class Banking: Data
    {
        public void AddBanking(string holder, string type, string bankName, string accountNo, string cvv, string expiry, string provider)
        {
            EnsureConnectionOpen();

            string query = "INSERT INTO BankingDetails (HolderName, AccountType, BankName, AccountNo, CVV, ExpiryDate, ProviderID) " +
                            "VALUES (@HolderName, @AccountType, @BankName, @AccountNo, @CVV, @ExpiryDate, @ProviderID)";

            using(SqlCommand  cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@HolderName", holder);
                cmd.Parameters.AddWithValue("@AccountType", type);
                cmd.Parameters.AddWithValue("@BankName", bankName);
                cmd.Parameters.AddWithValue("@AccountNo", accountNo);
                cmd.Parameters.AddWithValue("@CVV", cvv);
                cmd.Parameters.AddWithValue("@ExpiryDate", expiry);
                cmd.Parameters.AddWithValue("@ProviderID", provider);

                // Execute the query
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class ServiceSignup : Data
    {
        // Method to insert a new user into the ProviderSignUp table
        public void AddUser(string providerID, string firstName, string surname, string gender, string idNo, DateTime regdate,
                            string phone, string email, string service, string regno, string password, string name, string description, string location)
        {
            EnsureConnectionOpen();

            string query = "INSERT INTO ProviderSignUp (ProviderID, FirstName, Surname, Gender, IDno, PhoneNumber, " +
                           "EmailAddress, Service, BusinessRegNo,RegDate, Password, BusinessName, Description, Location) " +
                           "VALUES(@ProviderID, @FirstName, @Surname, @Gender, @IDno, @PhoneNumber, @EmailAddress, @Service, @BusinessRegNo, @RegDate, @Password, @BusinessName, @Description, @Location)";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                // Add parameters
                cmd.Parameters.AddWithValue("@ProviderID", providerID);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@Surname", surname);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@IDno", idNo);
                cmd.Parameters.AddWithValue("@PhoneNumber", phone);
                cmd.Parameters.AddWithValue("@EmailAddress", email);
                cmd.Parameters.AddWithValue("@Service", service);
                cmd.Parameters.AddWithValue("@BusinessRegNo", regno);
                cmd.Parameters.AddWithValue("@BusinessName", name);
                cmd.Parameters.AddWithValue("@RegDate", regdate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@Location", location);

                // Execute the query
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class ProviderLoader : Data
    {
        // Method to load user data into textboxes based on ProviderID
        public void LoadUserData(string providerID, TextBox txtFirstName, TextBox txtSurname, ComboBox cmbGender,
                                 TextBox txtIDNo, TextBox txtPhone, TextBox txtEmail, ComboBox cmbService,
                                 TextBox txtLocation, DateTimePicker dtpRegDate, TextBox txtBusinessName, 
                                 TextBox txtBusinessRegNo, TextBox txtPrice, TextBox txtDescription, TextBox txtDuration, TextBox txtPassword)
        {
            EnsureConnectionOpen();
            string currentMembershipID = Session.ProviderID;
            string query = "SELECT FirstName, Surname, Gender, IDno, PhoneNumber, EmailAddress, Service, BusinessRegNo, " +
                       "RegDate, Password, BusinessName, Description, Location, Price, Duration FROM ProviderSignUp WHERE ProviderID = '" + currentMembershipID + "'";


            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@ProviderID", providerID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Load data into textboxes
                        txtFirstName.Text = reader["FirstName"].ToString();
                        txtSurname.Text = reader["Surname"].ToString();
                        txtIDNo.Text = reader["IDno"].ToString();
                        txtPhone.Text = reader["PhoneNumber"].ToString();
                        txtEmail.Text = reader["EmailAddress"].ToString();
                        txtBusinessRegNo.Text = reader["BusinessRegNo"].ToString();
                        txtPassword.Text = reader["Password"].ToString();
                        txtBusinessName.Text = reader["BusinessName"].ToString();
                        txtDescription.Text = reader["Description"].ToString();
                        txtLocation.Text = reader["Location"].ToString();
                        txtPrice.Text = reader["Price"].ToString();
                        txtDuration.Text = reader["Duration"].ToString();

                        // Load data into ComboBox
                        cmbGender.SelectedItem = reader["Gender"].ToString();
                        cmbService.SelectedItem = reader["Gender"].ToString();

                        // Load data into DateTimePicker
                        if (DateTime.TryParse(reader["RegDate"].ToString(), out DateTime regDate))
                        {
                            dtpRegDate.Value = regDate;
                        }
                        else
                        {
                            dtpRegDate.Value = DateTime.Now; // Set a default value if parsing fails
                        }
                    }
                    else
                    {
                        throw new Exception("No user found with the provided ProviderID.");
                    }
                }
            }
        }
    }

    public class UpdateServiceSignup : Data
    {
        // Method to update user information in the ProviderSignUp table
        public void UpdateUser(string providerID, string firstName, string surname, string gender, string idNo, DateTime regdate,
                            string phone, string email, string service, string regno, string name, string description, 
                            string location, string price, string duration, string password)
        {
            EnsureConnectionOpen();
            string currentMembershipID = Session.ProviderID;
            string query = "UPDATE ProviderSignUp SET " +
                   "FirstName = @FirstName, " +
                   "Surname = @Surname, " +
                   "Gender = @Gender, " +
                   "IDno = @IDno, " +
                   "PhoneNumber = @PhoneNumber, " +
                   "EmailAddress = @EmailAddress, " +
                   "Service = @Service, " +
                   "BusinessRegNo = @BusinessRegNo, " +
                   "RegDate = @RegDate, " +
                   "BusinessName = @BusinessName, " +
                   "Description = @Description, " +
                   "Location = @Location, " +
                   "Price = @Price, " +
                   "Duration = @Duration, Password = @Password "+
                   "WHERE ProviderID = '" + currentMembershipID + "'";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                // Add parameters
                cmd.Parameters.AddWithValue("@ProviderID", providerID);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@Surname", surname);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@IDno", idNo);
                cmd.Parameters.AddWithValue("@PhoneNumber", phone);
                cmd.Parameters.AddWithValue("@EmailAddress", email);
                cmd.Parameters.AddWithValue("@Service", service);
                cmd.Parameters.AddWithValue("@BusinessRegNo", regno);
                cmd.Parameters.AddWithValue("@BusinessName", name);
                cmd.Parameters.AddWithValue("@RegDate", regdate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@Location", location);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@Duration", duration);
                cmd.Parameters.AddWithValue("@Password", password);

                // Execute the query
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class Feedback : Data
    {
        public void UpdateFeedback(string feedback, string rating)
        {
            EnsureConnectionOpen();

            string bookingID = Session.BookingID;
            if (string.IsNullOrEmpty(bookingID))
            {
                throw new InvalidOperationException("BookingID was not supplied.");
            }
            string feedbackQuery = "UPDATE Booking SET Feedback = @Feedback , Rating = @Rating WHERE BookingID = @BookingID";

            using (SqlCommand cmd = new SqlCommand(feedbackQuery, connection))
            {
                cmd.Parameters.AddWithValue("@Feedback", feedback);
                cmd.Parameters.AddWithValue("@Rating", rating);
                cmd.Parameters.AddWithValue("@BookingID", bookingID );

                cmd.ExecuteNonQuery();
            }
        }
    }

    public class DataRetriever : Data
    {
        // Method to load data based on a query and return it as a DataTable
        public DataTable LoadData(string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                EnsureConnectionOpen();  // Ensure the connection is open

                // Create a SqlDataAdapter to execute the query
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
                {
                    // Fill the DataTable with data
                    dataAdapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading data: " + ex.Message);
            }

            return dataTable;
        }
    }

    public class DataLabelLoader : Data
    {
        SqlDataReader reader;  // Keeps track of the reader for iteration

        private List<Dictionary<string, object>> records; // List to hold the records

        private int currentIndex; // Index of the current record

        // Method to load data from the database and display it in labels
        public void LoadDataIntoLabels(string query, System.Windows.Forms.Label[] labels)
        {
            try
            {
                SqlConnection connection = new SqlConnection(conString);
                
                connection.Open(); // Open the connection

                records = new List<Dictionary<string, object>>(); // Initialize records
                currentIndex = 0; // Reset the index

                SqlCommand cmd = new SqlCommand(query, connection);
                    
                reader = cmd.ExecuteReader();

                while (reader.Read()) // Read all records
                {
                    var record = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        record[reader.GetName(i)] = reader[i]; // Store record in dictionary
                    }
                    records.Add(record); // Add record to the list
                }

                if (records.Count > 0)
                {
                    UpdateLabels(labels, records[currentIndex]); // Update labels with the first record
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading data into labels: " + ex.Message);
            }
        }

        // Method to load the next record into labels (iteration)
        public void LoadNextDataIntoLabels(System.Windows.Forms.Label[] labels)
        {
            try
            {
                if (records != null && currentIndex < records.Count - 1)
                {
                    currentIndex++; // Move to the next record
                    UpdateLabels(labels, records[currentIndex]);
                }
                else
                {
                    MessageBox.Show("No more data found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading the next record: " + ex.Message);
            }
        }

        public void LoadPreviousDataIntoLabels(System.Windows.Forms.Label[] labels)
        {
            if (records != null && currentIndex > 0)
            {
                currentIndex--; // Move to the previous record
                UpdateLabels(labels, records[currentIndex]);
            }
            else
            {
                MessageBox.Show("No previous data found.");
            }
        }

        // Helper method to update the labels
        private void UpdateLabels(System.Windows.Forms.Label[] labels, Dictionary<string, object> record)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                if (i < record.Count)
                {
                    labels[i].Text = record.Values.ElementAt(i)?.ToString(); // Update label text
                }
            }
        }

    }

    public class Greet : Data
    {
        // Method to load data from the database and display it in labels
        public void LoadDataIntoLabels(string query)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open(); // Open the connection

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Reads only the first row of data
                            {
                                //lblGreetings.Text = "Hello," + reader["FirstName"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("No data found for the provided query.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading data into labels: " + ex.Message);
            }
        }
    }

}
