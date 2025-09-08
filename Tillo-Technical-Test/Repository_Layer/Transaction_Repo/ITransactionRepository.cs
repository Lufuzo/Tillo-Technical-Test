using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data_Layer.Models;

namespace Repository_Layer.Transaction_Repo
{
    public interface ITransactionRepository
    {
        void Initialize();
        Account GetOrCreateDefaultAccount();
        Account GetAccount(int id);
        void UpdateAccount(Account account);
        void AddTransaction(Transaction record);
        IEnumerable<Transaction> GetHistory(int top = 500);
    }
}
