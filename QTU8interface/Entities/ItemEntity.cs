using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QTU8interface.UFIDA;
namespace QTU8interface.Entities
{
    public class ItemEntity
    {
        public static ItemResult getItemResult(string expensecode, string itemCode, U8Login.clsLogin u8login)
        {
            ItemResult itemResult = new ItemResult();
            String itemclass = DBhelper.getDataFromSql(u8login.UfDbName, "select cass_item from code where ccode='" + expensecode + "' and iyear=" + u8login.cIYear);
            if (!string.IsNullOrEmpty(itemclass))
            {
                //string strSql = "select citemcode from fitemss" + itemclass + " where citemname='" + projname + prodname + "'";
                string strSql = "select citemcode from fitemss" + itemclass + " where citemcode='" + itemCode + "'";
                //LogHelper.WriteLog(typeof(ExpenseVouchEntity), strSql);
                String itemcode = DBhelper.getDataFromSql(u8login.UfDbName, strSql);
                if (string.IsNullOrEmpty(itemcode))
                {
                    itemResult.remsg = u8login.get_cAcc_Id() + "帐套 不存在项目档案：" + itemCode;                    
                    return itemResult;
                }
                else
                {
                    itemResult.remsg = "";
                    itemResult.itemclass = itemclass;
                    itemResult.itemcode = itemcode;
                }
            }
            else
            {
                itemResult.remsg = u8login.get_cAcc_Id() + "帐套 会计科目" + expensecode+"未设置项目辅助核算" + itemCode;
                return itemResult;
            }
            return itemResult;
        }
    }
}