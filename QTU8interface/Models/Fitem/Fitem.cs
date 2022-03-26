using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Fitem
{
    public class Fitem
    {
        public string ztcode { get; set; }//帐套编码
        public string oacode { get; set; }//项目编码
        public string oaname { get; set; }//项目名称
        public string classcode { get; set; }//项目分类编码
    }
}