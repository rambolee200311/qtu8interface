using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Receivable
{
    public class ClsReceivable
    {
        public string ztcode { get; set; }//帐套号 对应主体
        public Receivable_Head head { get; set; }
        public List<Receivable_Body> body { get; set; }
    }
}