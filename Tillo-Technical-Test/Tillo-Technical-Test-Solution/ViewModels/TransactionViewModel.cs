using Data_Layer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tillo_Technical_Test_Solution.ViewModels
{
    public class TransactionViewModel
    {
    

        public string TransactionId { get; set; } = Guid.NewGuid().ToString("N");
        public int AccountId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Receiver { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public TransactionStatus Status { get; set; } = TransactionStatus.Success;
    }
}