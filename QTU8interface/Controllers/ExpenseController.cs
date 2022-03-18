using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using QTU8interface.Models.Expense;
using QTU8interface.Models.Result;
using QTU8interface.UFIDA;
using QTU8interface.Entities;
namespace QTU8interface.Controllers
{
    public class ExpenseController : ApiController
    {
        // GET api/expense
        public ClsExpense Get()
        {
            //return new string[] { "value1", "value2" };
            ClsExpense ce = new ClsExpense();
            ce.ztcode = "996";
            ce.head = new Expense_Head();
            ce.head.oacode = "A8974922";
            ce.head.ddate =Convert.ToDateTime("2020-12-09");
            ce.head.dep = "市场销售部";
            ce.head.person = "张治宾";
            ce.head.accountcode = "321130100100421599";
            //ce.head.projname
            ce.body = new List<Expense_Body>();
            Expense_Body body1 = new Expense_Body();
            body1.rowno = 1;
            body1.bgcode = "07";
            body1.bgname = "招待费";
            body1.amount = 1000;
            body1.tax = 100;
            ce.body.Add(body1);

            Expense_Body body2 = new Expense_Body();
            body2.rowno = 2;
            body2.bgcode = "08";
            body2.bgname = "办公费";
            body2.amount = 800;
            body2.tax = 0;
            ce.body.Add(body2);

            //ce.settle = new List<Expense_Settle>();
            //Expense_Settle settle1 = new Expense_Settle();
            //settle1.rowno = 1;
            //settle1.accountcode = "321130100100421599";
            //ce.settle.Add(settle1);
            return ce;
        }

        // GET api/expense/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/expense
        public ClsResult Post([FromBody]ClsExpense expense)
        {
            LogHelper.WriteLog(typeof(ExpenseController),JsonHelper.ToJson(expense));
            ClsResult re = new ClsResult();
            re.oacode = expense.head.oacode;           
            re.ztcode = expense.ztcode;
            ExpenseVouchEntity.Add_Voucher(expense, ref re);
            
            LogHelper.WriteLog(typeof(ExpenseController), JsonHelper.ToJson(re));
            return re;
        }

        // PUT api/expense/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/expense/5
        public void Delete(int id)
        {
        }
    }
}
