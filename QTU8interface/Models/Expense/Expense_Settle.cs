using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Expense
{
    public class Expense_Settle
    {
        public int rowno { get; set; }//付款明细行号
        public string accountcode { get; set; }//付款方帐号
        public decimal amount { get; set; }//金额

    }
}