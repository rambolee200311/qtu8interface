using System;
using System.Collections.Generic;
using System.Web;

namespace QTU8interface.Models.Result
{
    public class ClsResult
    {
        public string ztcode { get; set; }//帐套编码
        public string oacode { get; set; }//oa编码
        public string u8code { get; set; }//u8编码
        public string recode { get; set; }//返回结果编码
        public string remsg { get; set; }//说明
    }
}