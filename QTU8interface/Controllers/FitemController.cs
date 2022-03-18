using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using QTU8interface.Models.Result;
using QTU8interface.Models.Fitem;
using QTU8interface.Entities;
using QTU8interface.UFIDA;

namespace QTU8interface.Controllers
{
    public class FitemController : ApiController
    {
        // GET api/fitem
        public Fitem Get()
        {
            //return new string[] { "value1", "value2" };
            Fitem fitem = new Fitem();
            fitem.oacode = "6986";
            fitem.oaname = "乱七八糟项目";
            fitem.ztcode = "996";
            fitem.classcode = "无分类";
            return fitem;
        }

        // GET api/fitem/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/fitem
        public ClsResult Post([FromBody]Fitem fitem)
        {
            LogHelper.WriteLog(typeof(FitemController),JsonHelper.ToJson(fitem));
            ClsResult re = new ClsResult();
            re.ztcode = fitem.ztcode;
            re.oacode = fitem.oaname;
            FitemEntity.Add_Archive(fitem, ref re);

            LogHelper.WriteLog(typeof(FitemController), JsonHelper.ToJson(re));
            return re;
        }

        // PUT api/fitem/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/fitem/5
        public void Delete(int id)
        {
        }
    }
}
