using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using QTU8interface.UFIDA;
using QTU8interface.Models.Result;
using QTU8interface.Models.Pay;
using QTU8interface.Entities;

namespace QTU8interface.Controllers
{
    public class PayController : ApiController
    {
        // GET api/pay
        public ClsPay Get()
        {
            //return new string[] { "value1", "value2" };
            ClsPay pay = new ClsPay();
            pay.ztcode = "996";
            pay.head = new Pay_Head();
            pay.head.oacode = "FKD8708709";
            pay.head.ddate = Convert.ToDateTime("2020-12-15");
            pay.head.person = "张福";
            pay.head.vendor = "北京凝丰科技有限公司";
            pay.head.projname = "腾讯AA项目";

            pay.body = new List<Pay_Body>();
            Pay_Body body1 = new Pay_Body();
            body1.rowno = 1;
            body1.amount = 100;
            body1.tax = 10;
            pay.body.Add(body1);
            Pay_Body body2 = new Pay_Body();
            body2.rowno = 2;
            body2.amount = 150;
            body2.tax = 0;
            pay.body.Add(body2);

            return pay;
        }

        // GET api/pay/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/pay
        public ClsResult Post([FromBody]ClsPay pay)
        {
            LogHelper.WriteLog(typeof(PayController), JsonHelper.ToJson(pay));
            ClsResult re = new ClsResult();
            re.ztcode = pay.ztcode;
            re.oacode = pay.head.oacode;
            PayEntity.Add_Pay(pay, ref re);
            //re.recode = "0";
            //re.remsg = "";
            //re.u8code = "7890709709";
            LogHelper.WriteLog(typeof(PayController), JsonHelper.ToJson(re));
            return re;
        }

        // PUT api/pay/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/pay/5
        public void Delete(int id)
        {
        }
    }
}
