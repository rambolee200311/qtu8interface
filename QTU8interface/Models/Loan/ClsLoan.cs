using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Loan
{
    public class ClsLoan
    {
        public string ztcode { get; set; }//帐套号 对应主体
        public Loan_Head head { get; set; }
        public List<Loan_Body> body { get; set; }
        //public List<Loan_Settle> settle { get; set; }

    }
}