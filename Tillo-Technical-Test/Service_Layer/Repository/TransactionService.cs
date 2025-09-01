using Data_Layer.Models;
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
        public Data_Layer.Models.Transaction Deposit(double amount, string description)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive");

            var tx = new Data_Layer.Models.Transaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                TimeStamp = DateTime.Now,
                Amount = amount,
                TransactionDescription = description,
                Type = "Deposit",
                Status = "Success"
            };

            InMemoryDb.Account.Balance += amount;
            InMemoryDb.Transactions.Add(tx);

            return tx;
        }

        
    }
}
