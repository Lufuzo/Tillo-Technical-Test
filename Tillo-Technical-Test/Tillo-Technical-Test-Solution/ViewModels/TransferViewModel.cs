using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tillo_Technical_Test_Solution.ViewModels
{
    public class TransferViewModel
    {
        public decimal Amount { get; set; }
        public string Receiver { get; set; }
        public string Description { get; set; }
    }
}