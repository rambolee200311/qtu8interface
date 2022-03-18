using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace QTU8interface.UFIDA
{
    public class LoginHelper
    {
        public static U8Login.clsLoginClass getU8LoginEntity(string accid)
        {

            U8Login.clsLoginClass m_ologin = new U8Login.clsLoginClass();
            XmlDocument xmlDoc = new XmlDocument();
            string user = "";
            string password = "";
            string server = "";

            try
            {
                xmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\MainConfig.xml");
                if (xmlDoc.SelectSingleNode("ufinterface/ACC[@id='" + accid + "']") != null)
                {
                    user = xmlDoc.SelectSingleNode("ufinterface/ACC[@id='" + accid + "']").SelectSingleNode("user").InnerText;
                    password = xmlDoc.SelectSingleNode("ufinterface/ACC[@id='" + accid + "']").SelectSingleNode("password").InnerText;
                    server = xmlDoc.SelectSingleNode("ufinterface/ACC[@id='" + accid + "']").SelectSingleNode("server").InnerText;
                }
                else
                {
                    return null;
                }
                string sYear = "";
                //string sPeriod = "";
                string sDate = "";
                if (accid == "999")
                {
                    sYear = "2015";
                    sDate = "2015-01-01";
                }
                else
                {
                    sYear = DateTime.Now.Year.ToString();
                    sDate = DateTime.Now.ToShortDateString();
                }

                if (!m_ologin.Login("AA", accid, sYear, user, password, sDate, server, ""))
                {
                    return null;                    
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(LoginHelper), ex);
                return null;
            }
            return m_ologin;
        }
    }
}