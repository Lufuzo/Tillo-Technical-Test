using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Layer.Models
{
    public class Transaction
    {
        public string TransactionId { get; set; } = string.Empty;
         public string TransactionDescription { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; }

        public double Amount { get; set; } = 0;

        public string Reciever { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;


    }
}
