﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using QTU8interface.Entities;
using QTU8interface.Models.Result;
using QTU8interface.Models.Receivable;
using QTU8interface.UFIDA;

namespace QTU8interface.Controllers
{
    public class ReceivableController : ApiController
    {
        // GET api/receivable
        public ClsReceivable Get()
        {
            //return new string[] { "value1", "value2" };
            ClsReceivable receivable = new ClsReceivable();
            receivable.ztcode = "996";
            receivable.head = new Receivable_Head();
            receivable.head.oacode = "YSD202203260021";
            receivable.head.ddate = Convert.ToDateTime("2021-01-31");
            receivable.head.person = "潘倩倩";
            receivable.head.customer = "北京墨迹风云科技股份有限公司";
            receivable.head.projname = "聚好看-容器-500";

            receivable.body = new List<Receivable_Body>();
            Receivable_Body body1 = new Receivable_Body();
            body1.rowno = 1;
            body1.amount = 1300;
            body1.tax = 300;
            body1.kplb = "产品类";
            receivable.body.Add(body1);
            Receivable_Body body2 = new Receivable_Body();
            body2.rowno = 2;
            body2.amount = 150;
            body2.tax = 0;
            body2.kplb = "其他类";
            receivable.body.Add(body2);

            return receivable;
        }

        // GET api/receivable/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/receivable
        public ClsResult Post([FromBody]ClsReceivable receivable)
        {
            LogHelper.WriteLog(typeof(ReceivableController), JsonHelper.ToJson(receivable));
            ClsResult re = new ClsResult();
            re.ztcode = receivable.ztcode;
            re.oacode = receivable.head.oacode;
            //PayableEntity.Add_Payable(payable, ref re);
            ReceivableEntity.Add_Receivable(receivable, ref re);
            //re.u8code = "08709709";
            //re.recode = "0";
            //re.remsg = "";
            LogHelper.WriteLog(typeof(ReceivableController), JsonHelper.ToJson(re));
            return re;
        }

        // PUT api/receivable/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/receivable/5
        public void Delete(int id)
        {
        }
    }
}
