using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using QTU8interface.Models.Vendor;
using QTU8interface.Models.Result;
using QTU8interface.UFIDA;

namespace QTU8interface.Entities
{
    public class VendorEntity
    {
        public static void Add_Archive(Vendor cust, ref ClsResult re)
        {
            XmlDocument xmlDoc = new XmlDocument();
            bool bResult = false;
            string strResult = "";
            string classcode = "";
            string code = "";
            try
            {
                xmlDoc.Load(HttpContext.Current.Server.MapPath("..") + "\\UFIDA\\vendor.xml");
                U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity(cust.ztcode.ToString());
                if (u8login == null)
                {
                    re.recode = "111";
                    re.remsg = re.ztcode + "对应帐套登录失败";
                    return;
                }
                if (DBhelper.getDataFromSql(u8login.UfDbName, "select 1 from vendor where cvenname='" + cust.oaname + "' or cmemo='" + cust.oacode + "'") != "")
                {
                    //re.remsg = cust.oaname + "已存在同名供应商";
                    //re.recode = "222";
                    //return;
                    code = DBhelper.getDataFromSql(u8login.UfDbName, "select cvenCode from vendor where cMemo='" + code + "'");
                    re.recode = "0";
                    re.remsg = "";
                    re.u8code = code;
                    return;
                }
                classcode = DBhelper.getDataFromSql(u8login.UfDbName, "select cvccode from VendorClass where bvCEnd=1 and cvcname='" + cust.classcode + "'");
                if (classcode == "")
                {
                    re.remsg = cust.classcode + "不存在供应商或非末级";
                    re.recode = "222";
                    return;
                }

                U8SrvTrans.IClsCommonClass ust = new U8SrvTrans.IClsCommonClass();
                ust.SetLogin(u8login);
                ust.Init(u8login.UfDbName);

                code = cust.oacode;


                xmlDoc.SelectSingleNode("data/vendor/cVenCode").InnerText = code;
                xmlDoc.SelectSingleNode("data/vendor/cVenHeadCode").InnerText = code;

                xmlDoc.SelectSingleNode("data/vendor/cVCCode").InnerText =classcode;

                xmlDoc.SelectSingleNode("data/vendor/cVenName").InnerText =cust.oaname;
                xmlDoc.SelectSingleNode("data/vendor/cVenAbbName").InnerText =cust.oaname;

                xmlDoc.SelectSingleNode("data/vendor/dVenDevDate").InnerText = DateTime.Now.ToShortDateString();
                xmlDoc.SelectSingleNode("data/vendor/dLastDate").InnerText = DateTime.Now.ToShortDateString();
                xmlDoc.SelectSingleNode("data/vendor/dLRDate").InnerText = DateTime.Now.ToShortDateString();
                xmlDoc.SelectSingleNode("data/vendor/dModifyDate").InnerText = DateTime.Now.ToShortDateString();
                xmlDoc.SelectSingleNode("data/vendor/dVenCreateDatetime").InnerText = DateTime.Now.ToShortDateString();

                xmlDoc.SelectSingleNode("data/vendor/cMemo").InnerText = code;
                
                bResult = ust.Add(xmlDoc.OuterXml, "Vendor", ref strResult);
                if (bResult)
                {
                    code = DBhelper.getDataFromSql(u8login.UfDbName, "select cvenCode from vendor where cMemo='" + code + "'");
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
                    LogHelper.WriteLog(typeof(VendorEntity), u8login.UfDbName);
                    return;
                }
            }
            catch (Exception ex)
            {
                re.recode = "999";
                re.remsg = ex.Message;
                LogHelper.WriteLog(typeof(VendorEntity), ex);
                return;
            }
        }
    }
}