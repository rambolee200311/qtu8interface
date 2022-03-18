using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QTU8interface.UFIDA;
using QTU8interface.Models.Receive;
using QTU8interface.Models.Result;

namespace QTU8interface.Entities
{
    public class ReceiveEntity
    {
        public static void Add_Pay(ClsReceive pay, ref ClsResult re)
        {
            int rowno = 1;
            ADODB.Connection conn = new ADODB.Connection();
            string strResult = "";
            string strSql = "";
            string depcode = "";
            string personcode = "";
            string itemcode = "";
            string vencode = "";
            decimal amount = 0m;
            decimal sumamount = 0m;
            decimal tax = 0m;
            decimal money = 0m;
            decimal taxrate = 0m;
            bool bTran = false;
            MSXML2.IXMLDOMDocument2 domHead = new MSXML2.DOMDocument30Class();
            MSXML2.IXMLDOMDocument2 domBody = new MSXML2.DOMDocument30Class();

            U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity(pay.ztcode.ToString());
            if (u8login == null)
            {
                re.recode = "111";
                re.remsg = re.ztcode + "对应帐套登录失败";
                return;
            }
            //检查应付单是否重复
            strResult = DBhelper.getDataFromSql(u8login.UfDbName, "select cdefine11 from ap_closebill where cdefine11='" + pay.head.oacode + "'");
            if (strResult != "")
            {
                strResult = pay.head.oacode + "已生成过付款单";
                re.recode = "333";
                re.remsg = strResult;
                return;
            }
            //检查人员是否存在
            depcode = DBhelper.getDataFromSql(u8login.UfDbName, "select cDept_num from hr_hi_person where cPsn_Name='" + pay.head.person + "'");
            personcode = DBhelper.getDataFromSql(u8login.UfDbName, "select cPsn_num from hr_hi_person where cPsn_Name='" + pay.head.person + "'");
            if (depcode == "")
            {
                strResult = pay.head.person + "在U8人员档案中不存在";
                re.recode = "222";
                re.remsg = strResult;
                return;
            }
            //检查项目是否存在
            if (!string.IsNullOrEmpty(pay.head.projname))
            {
                itemcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemcode from fitemss97 where citemname='" + pay.head.projname + "'");
                if (itemcode == "")
                {
                    strResult = pay.head.projname + "在U8项目管理档案中不存在";
                    re.recode = "222";
                    re.remsg = strResult;
                    return;
                }
            }
            //检查客户是否存在
            vencode = DBhelper.getDataFromSql(u8login.UfDbName, "select ccuscode from customer where ccusname='" + pay.head.customer + "'");
            if (vencode == "")
            {
                strResult = pay.head.customer + "在U8客户档案中不存在";
                re.recode = "222";
                re.remsg = strResult;
                return;
            }
            try
            {
                conn.Open(u8login.UfDbName);
                domHead.load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\ARCloseHeadADD.xml");
                domBody.load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\ARCloseBodyADD.xml");



                #region//body
                MSXML2.IXMLDOMNode xnnode = domBody.selectSingleNode("xml").selectSingleNode("rs:data").selectSingleNode("z:row");
                sumamount = 0m;
                rowno = Convert.ToInt32(DBhelper.getDataFromSql(u8login.UfDbName, "select  isnull(max(id),0)+1 from ap_closebills"));
                foreach (Receive_Body body in pay.body)
                {
                    amount = body.amount;
                    tax = body.tax;
                    money = amount - tax;
                    if (money != 0)
                    { taxrate = (tax / money) * 100; }
                    sumamount += amount;
                    MSXML2.IXMLDOMNode xnnodeclone = xnnode.cloneNode(true);
                    xnnodeclone.attributes.getNamedItem("iID").text = DBhelper.getDataFromSql(u8login.UfDbName, "select  isnull(max(iid),0)+1 from ap_closebill");
                    xnnodeclone.attributes.getNamedItem("ID").text = rowno.ToString();
                    rowno++;
                    xnnodeclone.attributes.getNamedItem("cCusVen").text = vencode;
                    xnnodeclone.attributes.getNamedItem("iAmt").text = amount.ToString();
                    xnnodeclone.attributes.getNamedItem("iAmt_f").text = amount.ToString();
                    xnnodeclone.attributes.getNamedItem("iRAmt").text = money.ToString();
                    xnnodeclone.attributes.getNamedItem("iRAmt_f").text = money.ToString();
                    //xnnodeclone.attributes.getNamedItem("iTax").text = tax.ToString();
                    //xnnodeclone.attributes.getNamedItem("iNatTax").text = tax.ToString();
                    //xnnodeclone.attributes.getNamedItem("iTaxRate").text = taxrate.ToString();

                    xnnodeclone.attributes.getNamedItem("cDepCode").text = depcode;
                    xnnodeclone.attributes.getNamedItem("cPersonCode").text = personcode;
                    //xnnodeclone.attributes.getNamedItem("cItemCode").text = itemcode;
                    //xnnodeclone.attributes.getNamedItem("cItem_Class").text = "97";
                    xnnodeclone.attributes.getNamedItem("cDigest").text = body.memo;
                    xnnodeclone.attributes.getNamedItem("cMemo").text = body.memo;
                    xnnodeclone.attributes.getNamedItem("cDefine22").text = pay.head.oacode;
                    //xnnodeclone.attributes.getNamedItem("cDefine23").text = pay.head.billno;

                    domBody.selectSingleNode("xml").selectSingleNode("rs:data").appendChild(xnnodeclone);

                }
                if (domBody.selectSingleNode("xml").selectSingleNode("rs:data").childNodes.length > 1)
                {
                    domBody.selectSingleNode("xml").selectSingleNode("rs:data").removeChild(xnnode);
                }
                #endregion

                #region//head
                MSXML2.IXMLDOMNode xnnodehead = domHead.selectSingleNode("xml").selectSingleNode("rs:data").selectSingleNode("z:row");
                xnnodehead.attributes.getNamedItem("cDwCode").text = vencode;
                xnnodehead.attributes.getNamedItem("cDeptCode").text = depcode;
                xnnodehead.attributes.getNamedItem("cPerson").text = personcode;
                //xnnodehead.attributes.getNamedItem("cItemCode").text = itemcode;
                //xnnodehead.attributes.getNamedItem("cItem_Class").text = "97";

                xnnodehead.attributes.getNamedItem("iAmount").text = sumamount.ToString();
                xnnodehead.attributes.getNamedItem("iAmount_f").text = sumamount.ToString();
                xnnodehead.attributes.getNamedItem("iRAmount").text = sumamount.ToString();
                xnnodehead.attributes.getNamedItem("iRAmount_f").text = sumamount.ToString();

                xnnodehead.attributes.getNamedItem("cDefine11").text = pay.head.oacode;
                xnnodehead.attributes.getNamedItem("cDefine12").text = pay.head.accountcode;
                xnnodehead.attributes.getNamedItem("cDigest").text = pay.head.memo;
                xnnodehead.attributes.getNamedItem("dVouchDate").text = pay.head.ddate.ToShortDateString();
                xnnodehead.attributes.getNamedItem("iPeriod").text = pay.head.ddate.Month.ToString();
                xnnodehead.attributes.getNamedItem("iID").text = DBhelper.getDataFromSql(u8login.UfDbName, "select  isnull(max(iid),0)+1 from ap_closebill");

                #endregion

                NetCWAPI.U8NetCWAPIClass uncw = new NetCWAPI.U8NetCWAPIClass();
                uncw.NetCWToVBAdd(u8login.userToken, conn, "SaveVouch", "收款单", domHead.xml, domBody.xml, ref bTran, ref strResult);

                if (string.IsNullOrEmpty(strResult))
                {
                    re.recode = "0";
                    re.remsg = "";
                    re.u8code = DBhelper.getDataFromSql(u8login.UfDbName, "select cvouchid from ap_closebill where cdefine11='" + pay.head.oacode + "'");
                    return;
                }
                else
                {
                    re.recode = "888";
                    re.remsg = strResult;
                    //re.u8code = DBhelper.getDataFromSql(u8login.UfDbName, "select cvouchid from ap_closebill where cdefine1='" + pay.head.oacode + "'");
                    return;
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ReceiveEntity), ex);
                re.recode = "999";
                re.remsg = ex.Message;
                return;
            }
            finally
            {
                if (conn.State == 1)
                {
                    conn.Close();
                }
            }
        }
    }
}