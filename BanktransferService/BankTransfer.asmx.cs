using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace BanktransferService
{
    /// <summary>
    /// Summary description for BankTransfer
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class BankTransfer : System.Web.Services.WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["BankTransfer"].ConnectionString;

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public bool CheckUserLogin(string Username, string Password, out int UserID)
        {
            SqlConnection connection = null;
            int rowsAffected = 0;
            UserID = 0;
            try
            {
                connection = new SqlConnection(connectionString);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string queryString = "SELECT UserID FROM Users WHERE UserName = N'" + Username + "' AND UserPass = N'" + Password + "'";
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
                DataTable dataTable= new DataTable();
                adapter.Fill(dataTable);
                rowsAffected = dataTable.Rows.Count;
                UserID = (int)dataTable.Rows[0]["UserID"];
            } catch (Exception e)
            {
                Console.WriteLine("CheckUserLogin Exception: " + e.Message);
            } finally
            {
                if (connection != null) connection.Close();
            }
            return rowsAffected > 0;

        }

        [WebMethod]
        public bool CheckAccount(int AccountIDReceiver, int AccountIDRequest)
        {
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string queryString = "SELECT * FROM Accounts WHERE AccountID = " + AccountIDReceiver;
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                if (dataTable.Rows.Count > 0)
                {
                    if (AccountIDReceiver != AccountIDRequest)
                    {
                        return true;
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine("CheckAccount Exception: " + e.Message);
            } finally
            {
                if (connection != null) connection.Close();
            }
            return false;
        }

        [WebMethod]
        public DataSet GetAccountsByUserID(int userID)
        {

            SqlConnection connection = null;
            DataSet dataSet = null;
            try
            {
                connection = new SqlConnection(connectionString);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string queryString = "SELECT * FROM Accounts WHERE UserID = " + userID;
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
                dataSet = new DataSet();
                adapter.Fill(dataSet);
            }
            catch (Exception e)
            {
                Console.WriteLine("GetAccountsByUserID Exception: " + e.Message);
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return dataSet;
        }

        [WebMethod]
        public DataSet GetUsersInfor(int userID)
        {

            SqlConnection connection = null;
            DataSet dataSet = null;
            try
            {
                connection = new SqlConnection(connectionString);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string queryString = "SELECT * FROM Users WHERE UserID = " + userID;
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
                dataSet = new DataSet();
                adapter.Fill(dataSet);
            }
            catch (Exception e)
            {
                Console.WriteLine("GetUsersInfor Exception: " + e.Message);
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return dataSet;
        }

        [WebMethod]
        public DataSet GetUsersInforByAccounts(int accountID)
        {

            SqlConnection connection = null;
            DataSet dataSet = null;
            try
            {
                connection = new SqlConnection(connectionString);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string queryString = "SELECT Users.*, Accounts.AccountID, Accounts.AccountType, Accounts.TotalAmount FROM Users LEFT JOIN Accounts" +
                    " ON Accounts.UserID=Users.UserID WHERE Accounts.AccountID = " + accountID;
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
                dataSet = new DataSet();
                adapter.Fill(dataSet);
            }
            catch (Exception e)
            {
                Console.WriteLine("GetUsersInforByAccounts Exception: " + e.Message);
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return dataSet;
        }

        [WebMethod]
        public DataSet GetTransactionLog(int AccountID)
        {

            SqlConnection connection = null;
            DataSet dataSet = null;
            try
            {
                connection = new SqlConnection(connectionString);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string queryString = "SELECT * FROM Transactions WHERE RequestID = " + AccountID;
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
                dataSet = new DataSet();
                adapter.Fill(dataSet);
            }
            catch (Exception e)
            {
                Console.WriteLine("GetTransactionLog Exception: " + e.Message);
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return dataSet;
        }

        [WebMethod]
        public bool Transfer(int requestID, int receiverID, decimal amount, string Reason)
        {
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string queryString = "SELECT TotalAmount FROM Accounts WHERE AccountID = " + requestID;
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                if (dataTable.Rows.Count > 0)
                {
                    decimal amountFromAccount = (decimal)dataTable.Rows[0]["TotalAmount"];
                    if (amountFromAccount > amount)
                    {
                        // Trừ tiền người gửi
                        queryString = "UPDATE Accounts SET TotalAmount = TotalAmount - @Amount WHERE AccountID = @AccountID";
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.CommandType = CommandType.Text;

                        command.Parameters.Add("@Amount", SqlDbType.Money);
                        command.Parameters["@Amount"].Value = amount;
                        command.Parameters.Add("@AccountID", SqlDbType.Int);
                        command.Parameters["@AccountID"].Value = requestID;
                        command.ExecuteNonQuery();

                        // Cộng tiền người nhận
                        queryString = "UPDATE Accounts SET TotalAmount = TotalAmount + @Amount WHERE AccountID = @AccountID";
                        SqlCommand commandUpdate = new SqlCommand(queryString, connection);
                        commandUpdate.CommandText = queryString;
                        commandUpdate.Parameters.Add("@Amount", SqlDbType.Money);
                        commandUpdate.Parameters["@Amount"].Value = amount;
                        commandUpdate.Parameters.Add("@AccountID", SqlDbType.Int);
                        commandUpdate.Parameters["@AccountID"].Value = receiverID;
                        commandUpdate.ExecuteNonQuery();

                        // Thêm giao dịch
                        queryString = "INSERT INTO Transactions (RequestID, ReceiverID, Reason, Amount)" +
                            " VALUES (@RequestID, @ReceiverID, @Reason, @Amount)";
                        SqlCommand commandInsertTransactions = new SqlCommand(queryString, connection);
                        commandInsertTransactions.CommandText = queryString;
                        commandInsertTransactions.CommandType = CommandType.Text;
                        commandInsertTransactions.Parameters.Add("@RequestID", SqlDbType.Int);
                        commandInsertTransactions.Parameters["@RequestID"].Value = requestID;
                        commandInsertTransactions.Parameters.Add("@ReceiverID", SqlDbType.Int);
                        commandInsertTransactions.Parameters["@ReceiverID"].Value = receiverID;
                        commandInsertTransactions.Parameters.Add("@Reason", SqlDbType.NText);
                        commandInsertTransactions.Parameters["@Reason"].Value = Reason;
                        commandInsertTransactions.Parameters.Add("@Amount", SqlDbType.Money);
                        commandInsertTransactions.Parameters["@Amount"].Value = amount;
                        commandInsertTransactions.ExecuteNonQuery();

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetTransactionLog Exception: " + e.Message);
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return false;
        }
    }
}
