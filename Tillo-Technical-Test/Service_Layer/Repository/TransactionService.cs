using Data_Layer.Models;
using Repository_Layer.Transaction_Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;


namespace Service_Layer.Repository
{
    public class TransactionService : ITransactionService
    {

        private readonly ITransactionRepository _repo;
        private readonly int _accountId;

        private readonly string _connectionString = string.Empty;
        public TransactionService(ITransactionRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            var acc = _repo.GetOrCreateDefaultAccount();
            _accountId = acc.AccountId;
        }

        public decimal GetBalance() => _repo.GetAccount(_accountId).Balance;

        public IEnumerable<Data_Layer.Models.Transaction> History(int top = 500) => _repo.GetHistory(top);

        public Data_Layer.Models.Transaction Deposit(decimal amount, string description)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));

            var acc = _repo.GetAccount(_accountId);
            acc.Balance += amount;
            _repo.UpdateAccount(acc);

            var record = new Data_Layer.Models.Transaction
            {
                AccountId = _accountId,
                Type = TransactionType.Deposit,
                Amount = amount,
                Description = description ?? string.Empty,
                Status = Data_Layer.Models.TransactionStatus.Success,
                Timestamp = DateTime.UtcNow
            };
            _repo.AddTransaction(record);
            return record;
        }

        public Data_Layer.Models.Transaction Withdraw(decimal amount, string description)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
            var acc = _repo.GetAccount(_accountId);

            var record = new Data_Layer.Models.Transaction
            {
                AccountId = _accountId,
                Type = TransactionType.Withdrawal,
                Amount = amount,
                Description = description ?? string.Empty,
                Timestamp = DateTime.UtcNow
            };

            if (acc.Balance >= amount)
            {
                acc.Balance -= amount;
                _repo.UpdateAccount(acc);
                record.Status = Data_Layer.Models.TransactionStatus.Success;
            }
            else
            {
                record.Status = Data_Layer.Models.TransactionStatus.Failed;
            }

            _repo.AddTransaction(record);
            return record;
        }

        public Data_Layer.Models.Transaction Transfer(decimal amount, string receiver, string description)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
            if (string.IsNullOrWhiteSpace(receiver)) throw new ArgumentException("Receiver is required.", nameof(receiver));

            var acc = _repo.GetAccount(_accountId);

            var record = new Data_Layer.Models.Transaction
            {
                AccountId = _accountId,
                Type = TransactionType.Transfer,
                Amount = amount,
                Receiver = receiver,
                Description = description ?? string.Empty,
                Timestamp = DateTime.UtcNow
            };

            if (acc.Balance >= amount)
            {
                acc.Balance -= amount;
                _repo.UpdateAccount(acc);
                record.Status = Data_Layer.Models.TransactionStatus.Success;
            }
            else
            {
                record.Status = Data_Layer.Models.TransactionStatus.Failed;
            }

            _repo.AddTransaction(record);
            return record;
        }

      

    }
}
