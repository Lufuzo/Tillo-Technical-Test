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

        // Initialize DB and ensure schema
        public void Initialize()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                // Apply PRAGMAs
                using (var pragma = conn.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA journal_mode=WAL;";
                    pragma.ExecuteNonQuery();

                    pragma.CommandText = "PRAGMA busy_timeout=5000;";
                    pragma.ExecuteNonQuery();
                }

                // Create tables
                using (var cmd = conn.CreateCommand())
                {
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
                    );";
                    cmd.ExecuteNonQuery();
                }

                // Ensure default account exists
                using (var check = conn.CreateCommand())
                {
                    check.CommandText = "SELECT COUNT(1) FROM Accounts";
                    var count = Convert.ToInt32(check.ExecuteScalar());
                    if (count == 0)
                    {
                        using (var ins = conn.CreateCommand())
                        {
                            ins.CommandText = "INSERT INTO Accounts(Name, Balance) VALUES(@name, 0)";
                            ins.Parameters.AddWithValue("@name", "Default");
                            ins.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public Account GetOrCreateDefaultAccount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Balance FROM Accounts ORDER BY Id LIMIT 1";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                AccountId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Balance = Convert.ToDecimal(reader.GetDouble(2))
                            };
                        }
                    }
                }

                // Create new default if none found
                using (var ins = conn.CreateCommand())
                {
                    ins.CommandText = "INSERT INTO Accounts(Name, Balance) VALUES('Default', 0); SELECT last_insert_rowid();";
                    var id = Convert.ToInt32((long)ins.ExecuteScalar());
                    return new Account { AccountId = id, Name = "Default", Balance = 0m };
                }
            }
        }

        public Account GetAccount(int id)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Balance FROM Accounts WHERE Id=@id";
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                AccountId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Balance = Convert.ToDecimal(reader.GetDouble(2))
                            };
                        }
                    }
                }
            }
            throw new InvalidOperationException("Account not found.");
        }

        public void UpdateAccount(Account account)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Accounts SET Balance=@b WHERE Id=@id";
                    cmd.Parameters.AddWithValue("@b", account.Balance);
                    cmd.Parameters.AddWithValue("@id", account.AccountId);
                    cmd.ExecuteNonQuery();

                    tx.Commit();
                }
            }
        }

        public void AddTransaction(Transaction record)
        {
            if (string.IsNullOrEmpty(record.TransactionId))
                record.TransactionId = Guid.NewGuid().ToString();

            record.Timestamp = DateTime.UtcNow;

            // Apply business logic before saving
            var status = ApplyBusinessRules(record);

            record.Status = status;

            // Save transaction
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
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
            }
        }

        private TransactionStatus ApplyBusinessRules(Transaction record)
        {
            if (record.Amount <= 0)
                return TransactionStatus.Failed;

            try
            {
                var account = GetAccount(record.AccountId);

                switch (record.Type)
                {
                    case TransactionType.Deposit:
                        account.Balance += record.Amount;
                        UpdateAccount(account);
                        return TransactionStatus.Success;

                    case TransactionType.Withdrawal:
                        if (account.Balance < record.Amount)
                            return TransactionStatus.Failed;

                        account.Balance -= record.Amount;
                        UpdateAccount(account);
                        return TransactionStatus.Success;

                    case TransactionType.Transfer:
                        if (string.IsNullOrWhiteSpace(record.Receiver))
                            return TransactionStatus.Failed;

                        if (account.Balance < record.Amount)
                            return TransactionStatus.Failed;

                        account.Balance -= record.Amount;
                        UpdateAccount(account);
                        return TransactionStatus.Success;

                    default:
                        return TransactionStatus.Failed;
                }
            }
            catch
            {
                return TransactionStatus.Failed;
            }
        }

        public IEnumerable<Transaction> GetHistory(int top = 500)
        {
            var list = new List<Transaction>();
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                // Get column list from PRAGMA
                var availableColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var pragma = conn.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA table_info(Transactions);";
                    using (var reader = pragma.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            availableColumns.Add(reader["name"].ToString());
                        }
                    }
                }

                // Build query only with existing columns
                var selectedCols = new List<string> { "Id", "Amount", "Type", "Status" };

                if (availableColumns.Contains("AccountId")) selectedCols.Add("AccountId");
                if (availableColumns.Contains("Timestamp")) selectedCols.Add("Timestamp");
                else if (availableColumns.Contains("Date")) selectedCols.Add("Date"); // fallback
                if (availableColumns.Contains("Description")) selectedCols.Add("Description");
                if (availableColumns.Contains("Receiver")) selectedCols.Add("Receiver");
                if (availableColumns.Contains("TransactionId")) selectedCols.Add("TransactionId");
                if (availableColumns.Contains("BalanceAfter")) selectedCols.Add("BalanceAfter");

                string sql = $@"
            SELECT {string.Join(", ", selectedCols)}
            FROM Transactions
            ORDER BY datetime({(availableColumns.Contains("Timestamp") ? "Timestamp" : "Date")}) DESC
            LIMIT @top";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@top", top);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tx = new Transaction
                            {
                                TransactionId = availableColumns.Contains("TransactionId")
                                                ? reader["TransactionId"].ToString()
                                                : reader["Id"].ToString(),

                                Timestamp = (availableColumns.Contains("Timestamp") || availableColumns.Contains("Date"))
                                            && DateTime.TryParse(reader[availableColumns.Contains("Timestamp") ? "Timestamp" : "Date"]?.ToString(), out var ts)
                                            ? ts
                                            : DateTime.UtcNow,

                                Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0,
                                Type = Enum.TryParse<TransactionType>(reader["Type"]?.ToString(), true, out var tType) ? tType : TransactionType.Deposit,
                                Status = Enum.TryParse<TransactionStatus>(reader["Status"]?.ToString(), true, out var tStatus) ? tStatus : TransactionStatus.Success,
                                Description = availableColumns.Contains("Description") ? reader["Description"]?.ToString() : null,
                                Receiver = availableColumns.Contains("Receiver") ? reader["Receiver"]?.ToString() : null,
                                AccountId = availableColumns.Contains("AccountId") ? Convert.ToInt32(reader["AccountId"]) : 0
                            };

                            list.Add(tx);
                        }
                    }
                }
            }

            return list;
        }

    }

}
