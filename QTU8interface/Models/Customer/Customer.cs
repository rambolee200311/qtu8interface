using System;
using System.Collections.Generic;
using System.Web;

namespace QTU8interface.Models.BaseArchive.Cusotmer
{
    public class Customer
    {
        public string ztcode { get; set; }//帐套编码
        public string oacode { get; set; }//客户编码
        public string oaname { get; set; }//客户名称
        public string classcode { get; set; }//客户分类编码
        public string dccode { get; set; }//地区分类
    }
}