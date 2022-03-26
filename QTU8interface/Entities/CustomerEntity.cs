using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using QTU8interface.Models.BaseArchive.Cusotmer;
using QTU8interface.Models.Result;
using QTU8interface.UFIDA;
namespace QTU8interface.Entities
{
    public class CustomerEntity
    {
        public static void Add_Archive(Customer cust,ref ClsResult re)
        {
            XmlDocument xmlDoc = new XmlDocument();
            bool bResult = false;
            string strResult = "";
            string classcode = "";
            string dccode = "";
            string code = "";
            try
            {
                xmlDoc.Load(HttpContext.Current.Server.MapPath("..") + "\\UFIDA\\customer.xml");
                U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity(cust.ztcode.ToString(),"","");
                if (u8login == null)
                {
                    re.recode = "111";
                    re.remsg = re.ztcode + "对应帐套登录失败";
                    return;
                }
                if (DBhelper.getDataFromSql(u8login.UfDbName, "select 1 from Customer where ccusname='" + cust.oaname+ "' or cmemo='"+cust.oacode+"'") != "")
                {
                    //re.remsg = cust.oaname + "已存在同名客户";
                    //re.recode = "222";
                    //code = DBhelper.getDataFromSql(u8login.UfDbName, "select cCusCode from customer where ccusname='" + cust.oaname + "'");
                    re.recode = "0";
                    re.remsg = "";
                    re.u8code = DBhelper.getDataFromSql(u8login.UfDbName, "select cCusCode from customer where ccusname='" + cust.oaname + "'");
                    return;
                }
                classcode = DBhelper.getDataFromSql(u8login.UfDbName, "select ccccode from CustomerClass where bcCEnd=1 and cccname='" + cust.classcode + "'");
                if (classcode == "")
                {
                    re.remsg = cust.classcode + "不存在客户分类或非末级";
                    re.recode = "222";
                    return;
                }
                if (!string.IsNullOrEmpty(cust.dccode))
                {
                    dccode = DBhelper.getDataFromSql(u8login.UfDbName, "select cDCCode from DistrictClass where bDCEnd=1 and cDCName='" + cust.dccode + "'");
                    if (dccode == "")
                    {
                        re.remsg = cust.classcode + "不存在地区分类或非末级";
                        re.recode = "222";
                        return;
                    }
                }

                U8SrvTrans.IClsCommonClass ust = new U8SrvTrans.IClsCommonClass();
                ust.SetLogin(u8login);
                ust.Init(u8login.UfDbName);

                code = cust.oacode;
                xmlDoc.SelectSingleNode("data/customer/cCusCode").InnerText = code;
                xmlDoc.SelectSingleNode("data/customer/cCusHeadCode").InnerText = code;
                xmlDoc.SelectSingleNode("data/customer/cInvoiceCompany").InnerText = code;
                xmlDoc.SelectSingleNode("data/customer/cCusCreditCompany").InnerText = code;
                xmlDoc.SelectSingleNode("data/customer/iId").InnerText = code;
                xmlDoc.SelectSingleNode("data/customer/cCusName").InnerText = cust.oaname;
                xmlDoc.SelectSingleNode("data/customer/cCusAbbName").InnerText = cust.oaname;
                xmlDoc.SelectSingleNode("data/customer/cCCCode").InnerText = classcode;
                xmlDoc.SelectSingleNode("data/customer/dCusDevDate").InnerText = DateTime.Now.ToShortDateString();
                xmlDoc.SelectSingleNode("data/customer/dModifyDate").InnerText = DateTime.Now.ToShortDateString();
                xmlDoc.SelectSingleNode("data/customer/dModifyDate").InnerText = DateTime.Now.ToShortDateString();
                xmlDoc.SelectSingleNode("data/customer/cMemo").InnerText = code;
                xmlDoc.SelectSingleNode("data/customer/cDCCode").InnerText = dccode;
                bResult = ust.Add(xmlDoc.OuterXml, "Customer",ref strResult);
                if (bResult)
                {
                    code = DBhelper.getDataFromSql(u8login.UfDbName, "select cCusCode from customer where cMemo='" + code + "'");
                    re.recode = "0";
                    re.remsg = "";
                    re.u8code = code;
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(strResult))
                    {
                        re.remsg = "未知原因";
                    }
                    else
                    {
                        re.remsg = strResult;
                    }
                    re.recode = "333";
                    return;
                }
            }
            catch(Exception ex)
            {
                re.recode = "999";
                re.remsg = ex.Message;
                LogHelper.WriteLog(typeof(CustomerEntity), ex);
                return;
            }
        }
    }
}