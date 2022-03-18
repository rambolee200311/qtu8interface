using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Expense
{
    public class Expense_Head
    {
        public string oacode{get;set;}//oa报销单编号
        public DateTime ddate{get;set;}//报销日期
        public string dep{get;set;}//部门名称
        public string person{get;set;}//人员姓名
        public string prodname{get;set;}//产品名称 研发部门报销必填
        public string projname{get;set;}//项目名称 销售部门报销必填  
        public string accountcode { get; set; }//付款方帐号 
    }
}