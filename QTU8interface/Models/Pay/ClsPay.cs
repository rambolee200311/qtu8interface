using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Pay
{
    public class ClsPay
    {
        public string ztcode { get; set; }//帐套号 对应主体
        public Pay_Head head { get; set; }
        public List<Pay_Body> body { get; set; }
    }
}