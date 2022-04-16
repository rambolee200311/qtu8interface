using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using QTU8interface.UFIDA;
using QTU8interface.Models.Result;
using QTU8interface.Models.Payable;
using QTU8interface.Entities;

namespace QTU8interface.Controllers
{
    public class PayableController : ApiController
    {
        // GET api/payable
        public ClsPayable Get()
        {
            //return new string[] { "value1", "value2" };
            ClsPayable payable = new ClsPayable();
            payable.ztcode = "996";
            payable.head = new Payable_Head();
            payable.head.oacode = "YFD202101310031";
            payable.head.ddate = Convert.ToDateTime("2021-01-31");
            payable.head.person = "张娜";
            payable.head.vendor = "北京冠众科技有限公司";
            payable.head.projname = "心动网络-万相-2000";
            payable.head.yjkm = "办公费";
            payable.head.ejkm = "办公用品";
            payable.body = new List<Payable_Body>();
            Payable_Body body1 = new Payable_Body();
            body1.rowno = 1;
            body1.amount = 1200;
            body1.tax = 200;
            body1.person = "张娜";
            body1.projname = "心动网络-万相-2000";
            body1.yjkm = "办公费";
            body1.ejkm = "办公用品";
            payable.body.Add(body1);
            ;

            return payable;
        }

        // GET api/payable/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/payable
        public ClsResult Post([FromBody]ClsPayable payable)
        {
            LogHelper.WriteLog(typeof(PayableController), JsonHelper.ToJson(payable));
            ClsResult re = new ClsResult();
            re.ztcode = payable.ztcode;
            re.oacode = payable.head.oacode;
            PayableEntity.Add_Payable(payable,ref re);
            //re.u8code = "08709709";
            //re.recode = "0";
            //re.remsg = "";
            LogHelper.WriteLog(typeof(PayableController), JsonHelper.ToJson(re));
            return re;
        }

        // PUT api/payable/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/payable/5
        public void Delete(int id)
        {
        }
    }
}
