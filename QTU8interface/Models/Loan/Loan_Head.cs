using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Loan
{
    public class Loan_Head
    {
        public string oacode { get; set; }//oa借款单编号
        public DateTime ddate { get; set; }//借款日期
        public string dep { get; set; }//部门名称
        public string person { get; set; }//人员姓名

        public string accountcode { get; set; }//付款方帐号
    }
}