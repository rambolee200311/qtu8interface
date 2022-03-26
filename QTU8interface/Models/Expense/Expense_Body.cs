using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Expense
{
    public class Expense_Body
    {
        public int rowno { get; set; }//明细行号
        public string bgcode { get; set; }//预算科目编码
        public string bgname { get; set; }//预算科目名称
        public decimal amount { get; set; }//金额（含税金额）
        public decimal tax { get; set; }//进项税额（默认0）
        public string memo { get; set; }//备注
        public string person { get; set; }//人员姓名
    }

}