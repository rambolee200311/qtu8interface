using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QTU8interface.Models.Receive
{
    public class ClsReceive
    {
        public string ztcode { get; set; }//帐套号 对应主体
        public Receive_Head head { get; set; }
        public List<Receive_Body> body { get; set; }
    }
}