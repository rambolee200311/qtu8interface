using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using QTU8interface.UFIDA;

namespace QTU8interface.Entities
{
    public class ExpenseCodeEntity
    {
        //取得科目项目大类编码和项目编码
      
        public static CodeResult getExpenseCode(string ztcode, string person, string bgcode, string prodname, string projname, U8Login.clsLogin u8login, ref string itemclass, ref string itemcode)
        {
            string expensecode = "";
            string cashitemcode = "";
            string deptype = "";
            string depname = "";
            CodeResult result=new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            try
            {
                xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\codeexchange.xml");
                deptype = DBhelper.getDataFromSql(u8login.UfDbName, "select cDept_num from hr_hi_person where cPsn_Name='" + person + "'");
                if (string.IsNullOrEmpty(deptype))
                {
                    result.remsg = ztcode + "帐套" + person + "在U8人员档案里不存在";
                    result.recode = "";
                    return result;
                }
                depname = DBhelper.getDataFromSql(u8login.UfDbName, "select cdepname from department where cdepcode='" + deptype + "'");
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/deptype/dep[@u8code='" + deptype + "']");
                if (xmlNo==null)
                {
                    result.remsg = ztcode + "帐套" + person + "所在部门未设置部门类型";
                    result.recode = "";
                    return result;
                }
                deptype = xmlNo.Attributes["type"].Value;
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/expensecode[@type='" + deptype + "']/code[@bgcode='"+bgcode+"']");
                if (xmlNo == null)
                {
                    result.remsg =ztcode+"帐套"+ deptype+ "部门类型"+"预算科目"+bgcode+"未设置对应会计科目";
                    result.recode = "";
                    return result;
                }
                else
                {
                    if (string.IsNullOrEmpty(xmlNo.Attributes["u8code"].Value)) 
                    {
                        result.remsg = ztcode + "帐套" + deptype + "部门类型" + "预算科目" + bgcode + "未设置对应会计科目";
                        result.recode = "";
                        return result;
                    }
                }
                //特殊部门处理
                expensecode = xmlNo.Attributes["u8code"].Value;
                if (xmlNo.Attributes["cashitemcode"] != null)
                {
                    cashitemcode = xmlNo.Attributes["cashitemcode"].Value;
                }
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/expensecode[@type='" + deptype + "']/code[@bgcode='" + bgcode + "' and @depname='"+depname+"']");
                if (xmlNo != null)
                { 
                    expensecode = xmlNo.Attributes["u8code"].Value;
                    if (xmlNo.Attributes["cashitemcode"] != null)
                    {
                        cashitemcode = xmlNo.Attributes["cashitemcode"].Value;
                    }
                
                }
                //else
                //{
                //    result.remsg = bgcode + "预算科目 "+depname+" 部门类型未设置对应会计科目";
                //    result.recode = "";
                //    return result;
                //}
                result.remsg = "";
                result.recode = expensecode;
                result.cashitemcode = cashitemcode;
                //itemclass itemcode
                itemclass = DBhelper.getDataFromSql(u8login.UfDbName, "select cass_item from code where ccode='" + expensecode + "' and iyear="+u8login.cIYear);
                if (!string.IsNullOrEmpty(itemclass))
                {
                    //string strSql = "select citemcode from fitemss" + itemclass + " where citemname='" + projname + prodname + "'";
                    string strSql = "select citemcode from fitemss" + itemclass + " where citemcode='" + projname + prodname + "'";
                    //LogHelper.WriteLog(typeof(ExpenseVouchEntity), strSql);
                    itemcode = DBhelper.getDataFromSql(u8login.UfDbName, strSql);
                    if (string.IsNullOrEmpty(itemcode))
                    {
                        result.remsg = ztcode + "帐套 不存在项目档案：" + projname + prodname;
                        result.recode = "";
                        return result;
                    }
                }

            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(typeof(ExpenseCodeEntity),ex);
                result.remsg = ex.Message;
                result.recode = "";
                return result;
            }

            return result;

        }
        
        public static CodeResult getTaxCode(string ztcode)
        {
            string expensecode = "";           
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            try
            {
                xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\codeexchange.xml");
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/taxcode");
                if (xmlNo != null)
                { 
                    expensecode = xmlNo.Attributes["u8code"].Value;
                    result.remsg = "";
                    result.recode = expensecode;
                    //return result;
                }
                else
                {
                    result.remsg = ztcode+"帐套未设置税金科目";
                    result.recode = "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ExpenseCodeEntity), ex);
                result.remsg = ex.Message;
                result.recode = "";
                return result;
            }
            return result;
        }

        public static CodeResult getAccountCode(string ztcode,string account)
        {
            string expensecode = "";
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            try
            {
                xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\codeexchange.xml");
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/accountcode[@account='"+account+"']");
                if (xmlNo != null)
                {
                    expensecode = xmlNo.Attributes["u8code"].Value;
                    result.remsg = "";
                    result.recode = expensecode;
                    //return result;
                }
                else
                {
                    result.remsg = ztcode + "帐套"+account+"未设置现金银行银行科目";
                    result.recode = "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ExpenseCodeEntity), ex);
                result.remsg = ex.Message;
                result.recode = "";
                return result;
            }
            return result;
        }
    
        public static CodeResult getOtherARCode(string ztcode)
        {
            string expensecode = "";
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            try
            {
                xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\codeexchange.xml");
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/otherarcode");
                if (xmlNo != null)
                {
                    expensecode = xmlNo.Attributes["u8code"].Value;
                    result.remsg = "";
                    result.recode = expensecode;
                    //return result;
                }
                else
                {
                    result.remsg = ztcode + "帐套未设置其他应收科目";
                    result.recode = "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ExpenseCodeEntity), ex);
                result.remsg = ex.Message;
                result.recode = "";
                return result;
            }
            return result;
        }
    
        public static CodeResult getExchangeWageCode(string ztcode,string expensecode)
        {
            //string expensecode = "";
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            try
            {
                xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\codeexchange.xml");
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/wagecodes/wagecode[@u8code1='"+expensecode+"']");
                if (xmlNo != null)
                {
                    expensecode = xmlNo.Attributes["u8code2"].Value;
                    result.remsg = "";
                    result.recode = expensecode;
                    //return result;
                }
                else
                {
                    result.remsg = ztcode + "帐套未设置福利费中转科目";
                    result.recode = "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ExpenseCodeEntity), ex);
                result.remsg = ex.Message;
                result.recode = "";
                return result;
            }
            return result;
        }
    }
   
}