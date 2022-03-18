using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Vendor
{
    public class Vendor
    {
        public string ztcode { get; set; }//帐套编码
        public string oacode { get; set; }//供应商编码
        public string oaname { get; set; }//供应商名称
        public string classcode { get; set; }//供应商分类编码
    }
}