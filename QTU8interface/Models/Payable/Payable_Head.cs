using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Payable
{
    public class Payable_Head
    {
        public string oacode { get; set; }//Oa应收单编号
        public DateTime ddate { get; set; }//单据日期
        public string dep { get; set; }//部门名称
        public string person { get; set; }//人员姓名
        public string vendor { get; set; }//供应商名称
        public string projname { get; set; }//项目名称
        public string prodname { get; set; }//产品名称
        public string billno { get; set; }//发票号
        public string memo { get; set; }//备注
        public string yjkm { get; set; }//一级科目
        public string ejkm { get; set; }//二级科目

    }
}