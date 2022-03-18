using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using QTU8interface.Models.Result;
using QTU8interface.Models.BaseArchive.Cusotmer;
using QTU8interface.Entities;
using QTU8interface.UFIDA;
namespace QTU8interface.Controllers
{
    public class CustomerController : ApiController
    {
        // GET api/customer
        public Customer Get()
        {
            //return new string[] { "value1", "value2" };
            Customer cust = new Customer();
            cust.ztcode = "996";
            cust.oacode = "A001";
            cust.oaname = "北京三大步六科技公司";
            cust.classcode = "业务";
            return cust;
        }

        // GET api/customer/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/customer
        public ClsResult Post([FromBody]Customer cust)
        {
            LogHelper.WriteLog(typeof(CustomerController), JsonHelper.ToJson(cust));
            ClsResult re = new ClsResult();
            re.oacode = cust.oacode;
            CustomerEntity.Add_Archive(cust, ref re);
            //re.u8code = "0300001";
            //re.ztcode = cust.ztcode;
            //re.recode = "0";
            //re.remsg = "";
            LogHelper.WriteLog(typeof(CustomerController), JsonHelper.ToJson(re));
            return re;
        }

        // PUT api/customer/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/customer/5
        public void Delete(int id)
        {
        }
    }
}
