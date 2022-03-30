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
        /*
         * 20220330 销项税金科目
         */
        public static CodeResult getXssjkm(String ztcode,String kplb)
        {
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            try
            {
                xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\codeexchange.xml");
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/arcodes/arcode/xssjkm[@kplb='" + kplb + "']");
                if (xmlNo == null)
                {
                    xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/arcodes/arcode/xssjkm[@kplb='default']");
                }
                if (xmlNo == null)
                {
                    result.remsg = ztcode + "帐套" + "arcodes/arcode 未配置销项税金科目，请检查";
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
        public static CodeResult getKzkm(String ztcode)
        {
            CodeResult result = new CodeResult();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            try
            {
                xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\codeexchange.xml");
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/company[@code='" + ztcode + "']/arcodes/arcode[@type='kzkm']");
                if (xmlNo == null)
                {
                    result.remsg = ztcode+"帐套"+"arcodes/arcode 未配置应收科目，请检查";
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