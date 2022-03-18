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
    public class LoanController : ApiController
    {
        // GET api/loan
        public ClsLoan Get()
        {
            //return new string[] { "value1", "value2" };
            ClsLoan loan = new ClsLoan();
            loan.ztcode = "996";
            loan.head = new Loan_Head();
            loan.head.oacode = "LO9870971390471";
            loan.head.ddate =Convert.ToDateTime( "2020-12-11");
            loan.head.dep = "市场销售部";
            loan.head.person = "张治宾";
            loan.head.accountcode = "321130100100421599";

            loan.body = new List<Loan_Body>();
            Loan_Body body1 = new Loan_Body();
            body1.rowno = 1;
            body1.amount = 1000;
            body1.memo="因公出差";
            loan.body.Add(body1);
            Loan_Body body2 = new Loan_Body();
            body2.rowno = 2;
            body2.amount = 2000;
            body2.memo = "因公出差";
            loan.body.Add(body2);

            //loan.settle = new List<Loan_Settle>();
            //Loan_Settle settle1 = new Loan_Settle();
            //settle1.accountcode = "321130100100421599";
            //settle1.amount = 3000;
            //settle1.rowno = 1;
            //loan.settle.Add(settle1);

            return loan;
        }

        // GET api/loan/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/loan
        public ClsResult Post([FromBody]ClsLoan loan)
        {
            LogHelper.WriteLog(typeof(LoanController), JsonHelper.ToJson(loan));
            ClsResult re = new ClsResult();
            re.oacode = loan.head.oacode;
            re.ztcode = loan.ztcode;
            //ExpenseVouchEntity.Add_Voucher(expense, ref re);
            LoanVouchEntity.Add_Voucher(loan, ref re,"loan");

            LogHelper.WriteLog(typeof(LoanController), JsonHelper.ToJson(re));
            return re;
        }

        // PUT api/loan/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/loan/5
        public void Delete(int id)
        {
        }
    }
}
