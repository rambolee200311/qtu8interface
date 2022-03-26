using System;
using System.Collections.Generic;
using System.Web;

namespace QTU8interface.Models.BaseArchive.Cusotmer
{
    public class CustomerClass
    {
        public string ztcode { get; set; }//帐套编码
        public string code { get; set; }//客户分类编码
        public string name { get; set; }//客户分类名称
        public string qgbcode { get; set; }//企管宝客户分类编码
    }
}