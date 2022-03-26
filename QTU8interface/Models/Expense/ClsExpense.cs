using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Expense
{
    public class ClsExpense
    {
        public string ztcode { get; set; }//帐套号 对应主体
        public Expense_Head head { get; set; }
        public List<Expense_Body> body { get; set; }
        //public List<Expense_Settle> settle { get; set; }
    }
}