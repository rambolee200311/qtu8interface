using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Loan
{
    public class Loan_Body
    {
        public int rowno { get; set; }//明细行号
        public decimal amount { get; set; }//金额
        public string memo { get; set; }//借款事由

    }
}