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
using System.IO;

namespace ProService.Forms
{
    public partial class frmBooking : UserControl
    {
        public frmBooking()
        {
            InitializeComponent();
            pnlInvoice.DragEnter += PnlInvoice_DragEnter;
            pnlInvoice.DragDrop += PnlInvoice_DragDrop;
        }

        private void PnlInvoice_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // Allow copying the file
            }
            else
            {
                e.Effect = DragDropEffects.None; // Disable drop for unsupported types
            }
        }
        public string uploadedFilePath = null; // This will track the uploaded file
        public void PnlInvoice_DragEnter(object sender, DragEventArgs e)
        {

            // Get the dropped files from the drag-and-drop operation
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Check if any files were dropped
            if (files != null && files.Length > 0)
            {
                // Get the path of the first file that was dropped
                string filePath = files[0];
                MessageBox.Show($"File dropped: {filePath}", "File Uploaded", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Validate the file extension to ensure it is a PDF file
                if (Path.GetExtension(filePath).ToLower() != ".pdf")
                {
                    // Show a warning message if the file is not a PDF
                    MessageBox.Show("Only PDF files are allowed.", "Invalid File Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;// Exit the method as the file is invalid
                }

                // If valid, store the file path for later use
                uploadedFilePath = filePath;
            }

        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        public string conString = "Data Source=Megan;Initial Catalog=ProConnect;" +
            "Integrated Security=True;";
        DataLabelLoader dataLabelLoader = new DataLabelLoader();
        private void frmBooking_Load(object sender, EventArgs e)
        {
            string currentProvider = Session.SelectedProvider; // Get the selected provider
            string selectQuery = "SELECT BusinessName, Duration, Price FROM ProviderSignUp WHERE ProviderID = '"+ currentProvider +"'";
            
            // Generate a unique BookingID
            string bookingID = "#" + new Random().Next(00000, 99999).ToString(); // Generate # and 5 digits
           
            try
            {//Retrieve data from ProviderSignUp and prepare to insert into Booking
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open(); // Open the connection

                    string businessName = "";
                    string duration = "";
                    string price = "";

                    // Retrieve data for the selected provider
                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, connection))
                    {
                        selectCmd.Parameters.AddWithValue("@ProviderID", currentProvider);
                        using (SqlDataReader reader = selectCmd.ExecuteReader()) // Execute and get reader
                        {
                            if (reader.Read())
                            {
                                // Get data from the ProviderSignUp table
                                businessName = reader["BusinessName"].ToString();
                                duration = reader["Duration"].ToString();
                                price = reader["Price"].ToString();
                            }
                            
                        } // Automatically closes the reader here
                    }

                    // Update labels
                    lblBusinessName.Text = businessName;
                    lblDuration.Text = duration;
                    lblPrice.Text = price;
                    lblBookingID.Text = bookingID;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Provider not found" + ex.Message);
            }
        }

        public void btnClose_Click(object sender, EventArgs e)
        {
            
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (uploadedFilePath != null)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();

                        string currentProvider = Session.SelectedProvider; // Get the selected provider
                        

                        string selectQuery = "SELECT BusinessName, Duration, Price FROM ProviderSignUp WHERE ProviderID = '" + currentProvider + "'";
                        string insertQuery = "INSERT INTO Booking (BookingID, MembershipID, ProviderID, Price, Duration, Date) VALUES (@BookingID, @MembershipID, @ProviderID, @Price, @Duration, @BookingDate)";
                        // Generate a unique BookingID
                        string bookingID = "#" + new Random().Next(00000, 99999).ToString(); // Generate # and 5 digits

                        string businessName = "";
                        string duration = "";
                        string price = "";

                        // Retrieve data for the selected provider
                        using (SqlCommand selectCmd = new SqlCommand(selectQuery, connection))
                        {
                            using (SqlDataReader reader = selectCmd.ExecuteReader()) // Execute and get reader
                            {
                                if (reader.Read())
                                {
                                    // Get data from the ProviderSignUp table
                                    businessName = reader["BusinessName"].ToString();
                                    duration = reader["Duration"].ToString();
                                    price = reader["Price"].ToString();
                                }
                                
                            } // Automatically closes the reader here
                        }
                        

                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@BookingID", bookingID);
                            insertCmd.Parameters.AddWithValue("@MembershipID", Session.MembershipID); // Use current MembershipID
                            insertCmd.Parameters.AddWithValue("@ProviderID", currentProvider); // Use the selected provider
                            insertCmd.Parameters.AddWithValue("@Price", price);
                            insertCmd.Parameters.AddWithValue("@Duration", duration);
                            insertCmd.Parameters.AddWithValue("@BookingDate", DateTime.Now); // Use current date and time

                            int rowsAffected = insertCmd.ExecuteNonQuery(); // Execute the insert command
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Booking created successfully!");
                            }
                            else
                            {
                                MessageBox.Show("Failed to create booking.");
                            }
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
