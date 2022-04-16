using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Pay
{
    public class Pay_Body
    {
        public int rowno { get; set; }//明细行号
        public string projname { get; set; }//项目名称
        public string prodname { get; set; }//产品名称
        public decimal amount { get; set; }//金额（含税金额）
        public decimal tax { get; set; }//进项税额
        public string memo { get; set; }//备注
        public string person { get; set; }//费用承担部门负责人
        public string yjkm { get; set; }//一级科目
        public string ejkm { get; set; }//二级科目

    }
}