using Data_Layer.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Repository_Layer.Transaction_Repo
{
    public class TransactionRepository : ITransactionRepository
    {

        private readonly string _connectionString;

        public TransactionRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }



        public void Initialize()
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Accounts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Balance REAL NOT NULL DEFAULT 0
                );
                CREATE TABLE IF NOT EXISTS Transactions (
                    Id TEXT PRIMARY KEY,
                    AccountId INTEGER NOT NULL,
                    Type TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    Receiver TEXT NULL,
                    Description TEXT NULL,
                    Timestamp TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    FOREIGN KEY(AccountId) REFERENCES Accounts(Id)
                );
            ";
            cmd.ExecuteNonQuery();

            // Seed default account if not exists
            using var check = conn.CreateCommand();
            check.CommandText = "SELECT COUNT(1) FROM Accounts";
            var count = Convert.ToInt32(check.ExecuteScalar());
            if (count == 0)
            {
                using var ins = conn.CreateCommand();
                ins.CommandText = "INSERT INTO Accounts(Name, Balance) VALUES(@name, 0)";
                ins.Parameters.AddWithValue("@name", "Default");
                ins.ExecuteNonQuery();
            }
        }

        public Account GetOrCreateDefaultAccount()
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Balance FROM Accounts ORDER BY Id LIMIT 1";
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Account
                {
                    AccountId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Balance = Convert.ToDecimal(reader.GetDouble(2))
                };
            }
            // Shouldn't happen after Initialize, but just in case:
            using var ins = conn.CreateCommand();
            ins.CommandText = "INSERT INTO Accounts(Name, Balance) VALUES('Default', 0); SELECT last_insert_rowid();";
            var id = Convert.ToInt32((long)ins.ExecuteScalar());
            return new Account { AccountId = id, Name = "Default", Balance = 0m };
        }

        public Account GetAccount(int id)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Balance FROM Accounts WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Account
                {
                    AccountId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Balance = Convert.ToDecimal(reader.GetDouble(2))
                };
            }
            throw new InvalidOperationException("Account not found.");
        }

        public void UpdateAccount(Account account)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Accounts SET Balance=@b WHERE Id=@id";
            cmd.Parameters.AddWithValue("@b", account.Balance);
            cmd.Parameters.AddWithValue("@id", account.AccountId);
            cmd.ExecuteNonQuery();
        }

        public void AddTransaction(Transaction record)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Transactions
                (Id, AccountId, Type, Amount, Receiver, Description, Timestamp, Status)
                VALUES (@id, @accountId, @type, @amount, @receiver, @desc, @ts, @status)";
            cmd.Parameters.AddWithValue("@id", record.TransactionId);
            cmd.Parameters.AddWithValue("@accountId", record.AccountId);
            cmd.Parameters.AddWithValue("@type", record.Type.ToString());
            cmd.Parameters.AddWithValue("@amount", record.Amount);
            cmd.Parameters.AddWithValue("@receiver", (object?)record.Receiver ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@desc", (object?)record.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ts", record.Timestamp.ToString("o"));
            cmd.Parameters.AddWithValue("@status", record.Status.ToString());
            cmd.ExecuteNonQuery();
        }

        public IEnumerable<Transaction> GetTransactions(int top = 500)
        {
            var list = new List<Transaction>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT Id, AccountId, Type, Amount, Receiver, Description, Timestamp, Status FROM Transactions ORDER BY datetime(Timestamp) DESC LIMIT {top}";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Transaction
                {
                    TransactionId = reader.GetString(0),
                    AccountId = reader.GetInt32(1),
                    Type = Enum.Parse<TransactionType>(reader.GetString(2)),
                    Amount = Convert.ToDecimal(reader.GetDouble(3)),
                    Receiver = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Description = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Timestamp = DateTime.Parse(reader.GetString(6), null, System.Globalization.DateTimeStyles.RoundtripKind),
                    Status = Enum.Parse<TransactionStatus>(reader.GetString(7))
                });
            }
            return list;
        }

    }
}
