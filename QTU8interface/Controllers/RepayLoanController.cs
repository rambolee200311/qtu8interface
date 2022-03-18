using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using QTU8interface.Models.Loan;
using QTU8interface.Models.Result;
using QTU8interface.UFIDA;
using QTU8interface.Entities;

namespace QTU8interface.Controllers
{
    public class RepayLoanController : ApiController
    {
        // GET api/repayloan
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/repayloan/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/repayloan
        public ClsResult Post([FromBody]ClsLoan loan)
        {
            LogHelper.WriteLog(typeof(RepayLoanController), JsonHelper.ToJson(loan));
            ClsResult re = new ClsResult();
            re.oacode = loan.head.oacode;
            re.ztcode = loan.ztcode;
            //ExpenseVouchEntity.Add_Voucher(expense, ref re);
            LoanVouchEntity.Add_Voucher(loan, ref re, "repayloan");

            LogHelper.WriteLog(typeof(RepayLoanController), JsonHelper.ToJson(re));
            return re;
        }

        // PUT api/repayloan/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/repayloan/5
        public void Delete(int id)
        {
        }
    }
}
