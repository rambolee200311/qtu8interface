using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Payable
{
    public class ClsPayable
    {
        public string ztcode { get; set; }//帐套号 对应主体
        public Payable_Head head { get; set; }
        public List<Payable_Body> body { get; set; }
    }
}