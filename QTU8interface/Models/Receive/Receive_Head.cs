using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Receive
{
    public class Receive_Head
    {
        public string oacode { get; set; }//Oa应收单编号
        public DateTime ddate { get; set; }//单据日期
        public string dep { get; set; }//部门名称
        public string person { get; set; }//人员姓名
        public string customer { get; set; }//客户名称
        public string projname { get; set; }//项目名称
        public string prodname { get; set; }//产品名称
        public string accountcode { get; set; }//发票号
        public string memo { get; set; }//备注

    }
}