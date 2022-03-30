using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Receivable
{
    public class Receivable_Body
    {
        public int rowno { get; set; }//明细行号
        public string projname { get; set; }//项目名称
        public string prodname { get; set; }//产品名称
        public decimal amount { get; set; }//金额（含税金额）
        public decimal tax { get; set; }//进项税额
        public string memo { get; set; }//备注
        public string htbh { get; set; }//合同编号
        public string qykh { get; set; }//签约客户
        public string zzkh { get; set; }//最终客户
        public decimal bhsje { get; set; }//签约客户
        public string sqr { get; set; }//申请人
        public string ywy { get; set; }//业务员
        public string lcbh { get; set; }//流程编号
        public string kplb { get; set; }//开票类别
        public string kpnr { get; set; }//开票内容
    }
}