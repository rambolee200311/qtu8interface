using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using QTU8interface.UFIDA;

namespace QTU8interface
{
    public partial class WebHelper : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write(getARvouch());
        }

        private string getARvouch()
        {
            string result = "";
            U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity("996", "", "");
            ADODB.Connection conn = new ADODB.Connection();
            conn.Open(u8login.UfDbName);
            bool bTran = false;
            MSXML2.IXMLDOMDocument2 domQuery = new MSXML2.DOMDocument30Class();
            string xmlHead = "";
            string xmlBody = "";
            MSXML2.IXMLDOMDocument2 domHead = new MSXML2.DOMDocument30Class();
            MSXML2.IXMLDOMDocument2 domBody = new MSXML2.DOMDocument30Class();
            domQuery.load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\P0Query.xml");
            NetCWAPI.U8NetCWAPIClass uncw = new NetCWAPI.U8NetCWAPIClass();
            uncw.NetCWToVBData(u8login.userToken, conn, "GetVouchData", "应付单", domQuery.xml, ref xmlHead, ref xmlBody, ref result);
            domHead.loadXML(xmlHead);
            domBody.loadXML(xmlBody);
            domHead.save("d:\\abc\\apvouchhead111.xml");
            domBody.save("d:\\abc\\apvouchbody111.xml");
            if (bTran)
            {
                result = "it is ok";
            }
            conn.Close();
            return result;
        }
        private string addARvouch()
        {
            string result = "";
            int vtid = 8054;
            bool bTran=false;
            U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity("996", "", "");
            ADODB.Connection conn=new ADODB.Connection();
            conn.Open(u8login.UfDbName);
            //UFAPBO.clsVouchFacadeClass vfc = new UFAPBO.clsVouchFacadeClass();
            //vfc.Init("R0", u8login);
            MSXML2.IXMLDOMDocument2 domQuery = new MSXML2.DOMDocument30Class();
            string xmlHead="";
            string xmlBody="";
            MSXML2.IXMLDOMDocument2 domHead=new MSXML2.DOMDocument30Class();
            MSXML2.IXMLDOMDocument2 domBody=new MSXML2.DOMDocument30Class();
            //vfc.GetVouchData("R00000000001", ref domHead, ref domBody, ref result);
            //UFAPBO.clsAPVouchClass apvc = new UFAPBO.clsAPVouchClass();
            domQuery.load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\R0Query.xml");
            //apvc.Init(u8login);
            //apvc.GetVouchData(domQuery.xml, ref domHead, ref domBody, ref vtid, ref result);
            NetCWAPI.U8NetCWAPIClass uncw = new NetCWAPI.U8NetCWAPIClass();
            //uncw.NetCWToVBData(u8login.userToken, conn, "GetVouchData", "应收单", domQuery.xml, ref xmlHead, ref xmlBody, ref result);
            //domHead.loadXML(xmlHead);
            //domBody.loadXML(xmlBody);
            //domHead.save("d:\\abc\\arvouchhead111.xml");
            //domBody.save("d:\\abc\\arvouchbody111.xml");
            domHead.load("d:\\abc\\arvouchhead111.xml");
            domBody.load("d:\\abc\\arvouchbody111.xml");
            uncw.NetCWToVBAdd(u8login.userToken, conn, "SaveVouch", "应收单", domHead.xml, domBody.xml, ref bTran, ref result);
            if (bTran)
            {
                result = "it is ok";
            }
            conn.Close();
            return result;
        }
    }
}