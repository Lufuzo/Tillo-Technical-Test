using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tillo_Technical_Test_Solution
{
    public partial class Transactions : System.Web.UI.Page
    {
        private readonly string _connectionString =
            WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTransactions();
                UpdateBalance();

            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadTransactions();
            UpdateBalance();
        }

        protected void btnSimulate_Click(object sender, EventArgs e)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Transactions (Date, Amount, Description, Type, Status) 
                    VALUES (@date, @amount, @desc, @type, @status)";
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                cmd.Parameters.AddWithValue("@amount", 359.63);
                cmd.Parameters.AddWithValue("@desc", "Simulated TopUp Transaction");
                cmd.Parameters.AddWithValue("@type", "TopUp");
                cmd.Parameters.AddWithValue("@status", "Completed");
                cmd.ExecuteNonQuery();
            }

            lblMessage.Text = "TopUp: R359.63 successful!";
            LoadTransactions();
            UpdateBalance();
        }

        private void LoadTransactions()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Date, Amount, Description, Type, Status FROM Transactions ORDER BY Date DESC";

                var da = new SQLiteDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                gvTransactions.DataSource = dt;
                gvTransactions.DataBind();
            }
        }

        private void UpdateBalance()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT SUM(Amount) FROM Transactions WHERE Status='Completed'";
                var result = cmd.ExecuteScalar();

                decimal balance = (result != DBNull.Value) ? Convert.ToDecimal(result) : 0;
                lblBalance.Text = "Current Balance: " + balance.ToString("C");
            }
        }
    }
}
