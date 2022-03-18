using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using QTU8interface.UFIDA;
using QTU8interface.Models.Result;
using QTU8interface.Models.Receive;
using QTU8interface.Entities;

namespace QTU8interface.Controllers
{
    public class ReceiveController : ApiController
    {
        // GET api/receive
        public ClsReceive Get()
        {
            //return new string[] { "value1", "value2" };
            ClsReceive pay = new ClsReceive();
            pay.ztcode = "996";
            pay.head = new Receive_Head();
            pay.head.oacode = "SKD8708709";
            pay.head.ddate = Convert.ToDateTime("2020-12-15");
            pay.head.person = "张福";
            pay.head.customer = "广东欧珀移动通信有限公司";
            pay.head.projname = "腾讯AA项目";

            pay.body = new List<Receive_Body>();
            Receive_Body body1 = new Receive_Body();
            body1.rowno = 1;
            body1.amount = 100;
            body1.tax = 10;
            pay.body.Add(body1);
            Receive_Body body2 = new Receive_Body();
            body2.rowno = 2;
            body2.amount = 150;
            body2.tax = 0;
            pay.body.Add(body2);

            return pay;
        }

        // GET api/receive/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/receive
        public ClsResult Post([FromBody]ClsReceive receive)
        {
            LogHelper.WriteLog(typeof(ReceiveController), JsonHelper.ToJson(receive));
            ClsResult re = new ClsResult();
            re.ztcode = receive.ztcode;
            re.oacode = receive.head.oacode;
            ReceiveEntity.Add_Pay(receive, ref re);
            //PayEntity.Add_Pay(pay, ref re);
            //re.recode = "0";
            //re.remsg = "";
            //re.u8code = "7890709709";
            LogHelper.WriteLog(typeof(ReceiveController), JsonHelper.ToJson(re));
            return re;
        }

        // PUT api/receive/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/receive/5
        public void Delete(int id)
        {
        }
    }
}
