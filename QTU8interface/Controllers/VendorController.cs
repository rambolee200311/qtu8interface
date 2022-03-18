using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using QTU8interface.UFIDA;
using QTU8interface.Models.Vendor;
using QTU8interface.Models.Result;
using QTU8interface.Entities;

namespace QTU8interface.Controllers
{
    public class VendorController : ApiController
    {
        // GET api/vendor
        public Vendor Get()
        {
            //return new string[] { "value1", "value2" };
            Vendor cust = new Vendor();
            cust.ztcode = "996";
            cust.oacode = "A001";
            cust.oaname = "北京三大步六科技公司";
            cust.classcode = "采购";
            return cust;
        }

        // GET api/vendor/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/vendor
        public ClsResult Post([FromBody]Vendor vendor)
        {
            LogHelper.WriteLog(typeof(VendorController), JsonHelper.ToJson(vendor));
            ClsResult re = new ClsResult();
            re.ztcode = vendor.ztcode;
            re.oacode = vendor.oacode;
            LogHelper.WriteLog(typeof(VendorController), JsonHelper.ToJson(re));
            VendorEntity.Add_Archive(vendor, ref re);
            return re;
        }

        // PUT api/vendor/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/vendor/5
        public void Delete(int id)
        {
        }
    }
}
