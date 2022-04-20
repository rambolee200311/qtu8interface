using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using QTU8interface.UFIDA;
namespace QTU8interface.Entities
{
    public class ARAPCodeEntity
    {
        
        static string filePath=AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\arapcode.xml";
        static string filePathDep = AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\codeexchange.xml";
        /*
         * 20220330 销项税金科目
         */
        public static CodeResult getXssjkm(string ztcode,string kplb)
        {
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            try
            {
                xmlDoc.Load(filePath);
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/arcodes/arcode/xssjkm[@kplb='" + kplb + "']");
                if (xmlNo == null)
                {
                    xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/arcodes/arcode/xssjkm[@kplb='default']");
                }
                if (xmlNo == null)
                {
                    result.remsg = ztcode + "帐套 " +kplb+ " arcodes/arcode 未配置销项税金科目，请检查";
                    LogHelper.WriteLog(typeof(ARAPCodeEntity), result.remsg);
                    result.recode = "";
                    return result;
                }
                result.remsg = "";
                result.recode = xmlNo.Attributes["ccode"].Value.ToString(); ;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ARAPCodeEntity), ex);
                result.remsg = ex.Message;
                result.recode = "";
                return result;
            }
            return result;
        }

         /*
         * 20220330 应收科目
         */
        public static CodeResult getKzkm(string ztcode, U8Login.clsLogin u8login)
        {
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            String ccode = "";
            try
            {
                xmlDoc.Load(filePath);
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/arcodes/arcode[@type='kzkm']");
                if (xmlNo == null)
                {
                    result.remsg = ztcode+"帐套"+"arcodes/arcode 未配置应收科目，请检查";
                    LogHelper.WriteLog(typeof(ARAPCodeEntity), result.remsg);
                    result.recode = "";
                    return result;
                }
                result.remsg = "";
                ccode = xmlNo.Attributes["ccode"].Value.ToString();
                result.recode =ccode;
                if (xmlNo.Attributes["cashitemcode"] != null)
                {
                    result.cashitemcode = xmlNo.Attributes["cashitemcode"].Value;
                }
                string itemClass = DBhelper.getDataFromSql(u8login.UfDbName, "select cass_item from code where ccode='" + ccode + "' and iyear=" + u8login.cIYear);
                if (!string.IsNullOrEmpty(itemClass))
                {
                    result.itemClass = itemClass;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ARAPCodeEntity), ex);
                result.remsg = ex.Message;
                result.recode = "";
                return result;
            }
            return result;
        }
        /*
         * 20220405 应付科目         
         */
        public static CodeResult getAPKzkm(string ztcode, string yjkm, string ejkm, U8Login.clsLogin u8login,string strType)
        {
            /*
             * 2022-04-15 strType pay or payable
             */ 
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            string ccode="";
            try
            {
                xmlDoc.Load(filePath);
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/apcodes/apcode/kzkms/kzkm[@yjkm='" + yjkm + "' and @ejkm='" + ejkm + "']");
                
                if (xmlNo == null)
                {
                    result.remsg = ztcode + "帐套 " +yjkm+ "--"+ejkm+ "  apcodes/kzkms/kzkm 未配置应付科目，请检查";
                    LogHelper.WriteLog(typeof(ARAPCodeEntity), result.remsg);
                    result.recode = "";
                    return result;
                }
                result.remsg = "";
                ccode = xmlNo.Attributes["u8code"].Value.ToString();
                if (strType == "payable")
                {
                    if (xmlNo.Attributes["prepaycode"]!=null)
                    {
                        ccode = xmlNo.Attributes["prepaycode"].Value.ToString();
                    }
                }

                result.recode=ccode;
                if (xmlNo.Attributes["cashitemcode"] != null)
                {
                    result.cashitemcode = xmlNo.Attributes["cashitemcode"].Value;
                }

                string itemClass = DBhelper.getDataFromSql(u8login.UfDbName, "select cass_item from code where ccode='" + ccode + "' and iyear="+u8login.cIYear);
                if (!string.IsNullOrEmpty(itemClass))
                {
                    result.itemClass = itemClass;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ARAPCodeEntity), ex);
                result.remsg = ex.Message;
                result.recode = "";
                return result;
            }
            return result;
        }

        /*
         * 20220405 成本费用科目
         */
        public static CodeResult getAPCbfykm(string ztcode, string yjkm, string ejkm,string sajkm,string sijkm,string person, U8Login.clsLogin u8login)
        {
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            string ccode = "";
            string deptype = "";
            string depname = "";
            
            try
            {

                #region//根据人员取得部门类型
                if (!string.IsNullOrEmpty(person))
                {
                    XmlDocument xmlDocDep = new XmlDocument();
                    xmlDocDep.Load(filePathDep);
                    deptype = DBhelper.getDataFromSql(u8login.UfDbName, "select cDept_num from hr_hi_person where cPsn_Name='" + person + "'");
                    if (string.IsNullOrEmpty(deptype))
                    {
                        result.remsg = ztcode + "帐套" + person + "在U8人员档案里不存在";
                        LogHelper.WriteLog(typeof(ARAPCodeEntity), result.remsg);
                        result.recode = "";
                        return result;
                    }
                    depname = DBhelper.getDataFromSql(u8login.UfDbName, "select cdepname from department where cdepcode='" + deptype + "'");
                    xmlNo = xmlDocDep.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/deptype/dep[@u8code='" + deptype + "']");
                    if (xmlNo == null)
                    {
                        result.remsg = ztcode + "帐套" + person + "所在部门未设置部门类型";
                        LogHelper.WriteLog(typeof(ARAPCodeEntity), result.remsg);
                        result.recode = "";
                        return result;
                    }
                    deptype = xmlNo.Attributes["type"].Value;
                }
                #endregion

                xmlDoc.Load(filePath);
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode
                    + "']/apcodes/apcode/cbfykms/cbfykm[@yjkm='" + yjkm 
                    + "' and @ejkm='" + ejkm
                    + "' and @sajkm='" + sajkm
                    + "' and @sijkm='" + sijkm
                    + "' and @deptype='" + deptype 
                    + "']");
                if (xmlNo == null)
                {
                    xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode
                   + "']/apcodes/apcode/cbfykms/cbfykm[@yjkm='" + yjkm
                   + "' and @ejkm='" + ejkm
                   + "' and @sajkm='" + sajkm
                   + "' and @sijkm='" + sijkm                  
                   + "']");
                }

                if (xmlNo == null)
                {
                    result.remsg = ztcode + "帐套 " + yjkm + "--" + ejkm + "--" + sajkm + "--" + sijkm + "--" + deptype + "  apcodes/cbfykms/cbfykm 未配置成本费用科目，请检查";
                    LogHelper.WriteLog(typeof(ARAPCodeEntity), result.remsg);
                    result.recode = "";
                    return result;
                }
                result.remsg = "";
                ccode = xmlNo.Attributes["u8code"].Value.ToString();
                result.recode = ccode;
                result.midcode = xmlNo.Attributes["midcode"].Value.ToString();
                string itemClass = DBhelper.getDataFromSql(u8login.UfDbName, "select cass_item from code where ccode='" + ccode + "' and iyear=" + u8login.cIYear);
                if (!string.IsNullOrEmpty(itemClass))
                {
                    result.itemClass = itemClass;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ARAPCodeEntity), ex);
                result.remsg = ex.Message;
                result.recode = "";
                return result;
            }
            return result;
        }
        /*
         * 20220330 销项税金科目
         */
        public static CodeResult getCgsjkm(string ztcode)
        {
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            try
            {
                xmlDoc.Load(filePath);
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/apcodes/apcode[@type='cgsjkm']");
                
                if (xmlNo == null)
                {
                    result.remsg = ztcode + "帐套 " +  " apcodes/apcode 未配置进项税金科目，请检查";
                    LogHelper.WriteLog(typeof(ARAPCodeEntity), result.remsg);
                    result.recode = "";
                    return result;
                }
                result.remsg = "";
                result.recode = xmlNo.Attributes["ccode"].Value.ToString(); ;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ARAPCodeEntity), ex);
                result.remsg = ex.Message;
                result.recode = "";
                return result;
            }
            return result;
        }
    }
}