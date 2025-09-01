using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Data_Layer.Models;

namespace Service_Layer.Repository
{
    public interface ITransactionService
    {

      Data_Layer.Models.Transaction Deposit(double amount, string description);
        //Transaction Withdraw(decimal amount, string description);
        //Transaction Transfer(decimal amount, string receiver, string description);
        //List<Transaction> GetTransactions();
    }
}
