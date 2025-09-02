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

        Data_Layer.Models.Transaction Deposit(decimal amount, string description);
        Data_Layer.Models.Transaction Withdraw(decimal amount, string description);
        Data_Layer.Models.Transaction Transfer(decimal amount, string receiver, string description);
        decimal GetBalance();
        IEnumerable<Data_Layer.Models.Transaction> History(int top = 500);
    }
}
