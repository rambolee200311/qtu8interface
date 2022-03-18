using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QTU8interface.UFIDA;
using QTU8interface.Models.Receivable;
using QTU8interface.Models.Result;


namespace QTU8interface.Entities
{
    public class ReceivableEntity
    {
        public static void Add_Payable(ClsReceivable payable, ref ClsResult re)
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

            U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity(payable.ztcode.ToString());
            if (u8login == null)
            {
                re.recode = "111";
                re.remsg = re.ztcode + "对应帐套登录失败";
                return;
            }
            //检查应收单是否重复
            strResult = DBhelper.getDataFromSql(u8login.UfDbName, "select cdefine11 from ap_vouch where cdefine11='" + payable.head.oacode + "'");
            if (strResult != "")
            {
                strResult = payable.head.oacode + "已生成过应收单";
                re.recode = "333";
                re.remsg = strResult;
                return;
            }
            //检查人员是否存在
            depcode = DBhelper.getDataFromSql(u8login.UfDbName, "select cDept_num from hr_hi_person where cPsn_Name='" + payable.head.person + "'");
            personcode = DBhelper.getDataFromSql(u8login.UfDbName, "select cPsn_num from hr_hi_person where cPsn_Name='" + payable.head.person + "'");
            if (depcode == "")
            {
                strResult = payable.head.person + "在U8人员档案中不存在";
                re.recode = "222";
                re.remsg = strResult;
                return;
            }
            //检查项目是否存在
            if (!string.IsNullOrEmpty(payable.head.projname))
            {
                itemcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemcode from fitemss97 where citemname='" + payable.head.projname + "'");
                if (itemcode == "")
                {
                    strResult = payable.head.projname + "在U8项目管理档案中不存在";
                    re.recode = "222";
                    re.remsg = strResult;
                    return;
                }
            }
            //检查客户是否存在
            vencode = DBhelper.getDataFromSql(u8login.UfDbName, "select ccuscode from customer where ccusname='" + payable.head.customer + "'");
            if (vencode == "")
            {
                strResult = payable.head.customer + "在U8客户档案中不存在";
                re.recode = "222";
                re.remsg = strResult;
                return;
            }
            try
            {
                conn.Open(u8login.UfDbName);
                domHead.load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\ARVouchHeadADD.xml");
                domBody.load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\ARVouchBodyADD.xml");



                #region//body
                MSXML2.IXMLDOMNode xnnode = domBody.selectSingleNode("xml").selectSingleNode("rs:data").selectSingleNode("z:row");
                sumamount = 0m;
                foreach (Receivable_Body body in payable.body)
                {
                    if (!string.IsNullOrEmpty(body.ywy))
                    {
                        depcode = DBhelper.getDataFromSql(u8login.UfDbName, "select cDept_num from hr_hi_person where cPsn_Name='" + body.ywy + "'");
                        personcode = DBhelper.getDataFromSql(u8login.UfDbName, "select cPsn_num from hr_hi_person where cPsn_Name='" + body.ywy + "'");
                        if ((depcode == "") || (personcode == ""))
                        {
                            strResult = body.ywy + "在U8人员档案中不存在";
                            re.recode = "222";
                            re.remsg = strResult;
                            return;
                        }
                    }
                    amount = body.amount;
                    tax = body.tax;
                    money = amount - tax;
                    if (money != 0)
                    { taxrate = (tax / money) * 100; }
                    sumamount += amount;
                    MSXML2.IXMLDOMNode xnnodeclone = xnnode.cloneNode(true);
                    xnnodeclone.attributes.getNamedItem("cDwCode").text = vencode;
                    xnnodeclone.attributes.getNamedItem("iAmount").text = amount.ToString();
                    xnnodeclone.attributes.getNamedItem("iAmount_f").text = amount.ToString();
                    xnnodeclone.attributes.getNamedItem("iNoTaxAmount_f").text = money.ToString();
                    xnnodeclone.attributes.getNamedItem("iNoTaxAmount").text = money.ToString();
                    xnnodeclone.attributes.getNamedItem("iTax").text = tax.ToString();
                    xnnodeclone.attributes.getNamedItem("iNatTax").text = tax.ToString();
                    xnnodeclone.attributes.getNamedItem("iTaxRate").text = taxrate.ToString();

                    xnnodeclone.attributes.getNamedItem("cDeptCode").text = depcode;
                    xnnodeclone.attributes.getNamedItem("cPerson").text = personcode;
                    xnnodeclone.attributes.getNamedItem("cItemCode").text = itemcode;
                    xnnodeclone.attributes.getNamedItem("cItem_Class").text = "97";
                    xnnodeclone.attributes.getNamedItem("cDigest").text =body.memo;

                    if (!string.IsNullOrEmpty(body.htbh))
                    { xnnodeclone.attributes.getNamedItem("cDefine22").text = body.htbh.ToString(); }//合同编号 表体自定义项1
                    if (!string.IsNullOrEmpty(body.qykh))
                    { xnnodeclone.attributes.getNamedItem("cDefine23").text = body.qykh.ToString(); }//签约客户 表体自定义项2
                    if (!string.IsNullOrEmpty(body.zzkh))
                    {xnnodeclone.attributes.getNamedItem("cDefine24").text = body.zzkh.ToString(); }//最终客户 表体自定义项3
                    if (!string.IsNullOrEmpty(body.bhsje.ToString()))
                    {xnnodeclone.attributes.getNamedItem("cDefine26").text = body.bhsje.ToString(); }//不含税金额	表体自定义项5
                    if (!string.IsNullOrEmpty(body.sqr))
                    {xnnodeclone.attributes.getNamedItem("cDefine25").text = body.sqr.ToString(); }//申请人	表体自定义项4
                    if (!string.IsNullOrEmpty(body.ywy))
                    {xnnodeclone.attributes.getNamedItem("cDefine28").text = body.ywy.ToString(); }//业务员	表体自定义项7
                    if (!string.IsNullOrEmpty(body.lcbh))
                    { xnnodeclone.attributes.getNamedItem("cDefine29").text = body.lcbh.ToString(); }//流程编号	表体自定义项8


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
                xnnodehead.attributes.getNamedItem("cItemCode").text = itemcode;
                xnnodehead.attributes.getNamedItem("cItem_Class").text = "97";

                xnnodehead.attributes.getNamedItem("iAmount").text = sumamount.ToString();
                xnnodehead.attributes.getNamedItem("iAmount_f").text = sumamount.ToString();
                xnnodehead.attributes.getNamedItem("iRAmount").text = sumamount.ToString();
                xnnodehead.attributes.getNamedItem("iRAmount_f").text = sumamount.ToString();

                xnnodehead.attributes.getNamedItem("cDefine11").text = payable.head.oacode;
                xnnodehead.attributes.getNamedItem("cDefine12").text = payable.head.billno;
                xnnodehead.attributes.getNamedItem("cDigest").text = payable.head.memo;
                xnnodehead.attributes.getNamedItem("dVouchDate").text = payable.head.ddate.ToShortDateString();
                #endregion

                NetCWAPI.U8NetCWAPIClass uncw = new NetCWAPI.U8NetCWAPIClass();
                uncw.NetCWToVBAdd(u8login.userToken, conn, "SaveVouch", "应收单", domHead.xml, domBody.xml, ref bTran, ref strResult);

                if (string.IsNullOrEmpty(strResult))
                {
                    re.recode = "0";
                    re.remsg = "";
                    re.u8code = DBhelper.getDataFromSql(u8login.UfDbName, "select cvouchid from ap_vouch where cdefine11='" + payable.head.oacode + "'");
                    return;
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(ReceivableEntity), ex);
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