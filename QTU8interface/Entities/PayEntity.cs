﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using QTU8interface.UFIDA;
using QTU8interface.Models.Pay;
using QTU8interface.Models.Result;
using QTU8interface.Models.CashFlow;
using System.Data;
namespace QTU8interface.Entities
{
    public class PayEntity
    {
        public static void Add_Pay(ClsPay pay,ref ClsResult re)
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
            string vouchID = "";
            string cLink = "";
            string cashitemcode = "";
            bool bTran = false;

            //LogHelper.WriteLog(typeof(PayEntity), "msxml2 start");
            MSXML2.IXMLDOMDocument2 domHead = new MSXML2.DOMDocument30Class();
            MSXML2.IXMLDOMDocument2 domBody = new MSXML2.DOMDocument30Class();
            //LogHelper.WriteLog(typeof(PayEntity), "u8login start");
            U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity(pay.ztcode.ToString(), pay.head.ddate.Year.ToString(), pay.head.ddate.ToShortDateString());
            if (u8login == null)
            {
                re.recode = "111";
                re.remsg = re.ztcode + "对应帐套登录失败";
                return;
            }
            //LogHelper.WriteLog(typeof(PayEntity), "checkrepeat start");
            //检查应付单是否重复
            strResult = DBhelper.getDataFromSql(u8login.UfDbName, "select cdefine11 from ap_closebill where cdefine11='" + pay.head.oacode + "'");
            if (strResult != "")
            {
                strResult = pay.head.oacode + "已生成过付款单";
                re.recode = "333";
                re.remsg = strResult;
                return;
            }
           
            //检查项目是否存在
            //if (!string.IsNullOrEmpty(pay.head.projname))
            //{
            //    itemcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemcode from fitemss97 where citemname='" + pay.head.projname + "'");
            //    if (itemcode == "")
            //    {
            //        strResult = pay.head.projname + "在U8项目管理档案中不存在";
            //        re.recode = "222";
            //        re.remsg = strResult;
            //        return;
            //    }
            //}
            //检查供应商是否存在
            vencode = DBhelper.getDataFromSql(u8login.UfDbName, "select cvencode from vendor where cvenname='" + pay.head.vendor + "'");
            if (vencode == "")
            {
                strResult = pay.head.vendor + "在U8供应商档案中不存在";
                re.recode = "222";
                re.remsg = strResult;
                return;
            }
            try
            {
                conn.Open(u8login.UfDbName);
                domHead.load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\APCloseHeadADD.xml");
                domBody.load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\APCloseBodyADD.xml");

                //LogHelper.WriteLog(typeof(PayEntity), "xml body start");

                #region//body
                MSXML2.IXMLDOMNode xnnode = domBody.selectSingleNode("xml").selectSingleNode("rs:data").selectSingleNode("z:row");
                sumamount = 0m;
                rowno = Convert.ToInt32(DBhelper.getDataFromSql(u8login.UfDbName,"select  isnull(max(id),0)+1 from ap_closebills"));
                foreach (Pay_Body body in pay.body)
                {
                    amount = body.amount;
                    tax = body.tax;
                    money = amount - tax;                    
                    if (money != 0)
                    { taxrate = (tax / money) * 100; }
                    sumamount += amount;
                    MSXML2.IXMLDOMNode xnnodeclone = xnnode.cloneNode(true);
                    xnnodeclone.attributes.getNamedItem("iID").text = DBhelper.getDataFromSql(u8login.UfDbName,"select  isnull(max(iid),0)+1 from ap_closebill");
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
                    //检查人员是否存在
                    if (!string.IsNullOrEmpty(body.person))
                    {
                        depcode = DBhelper.getDataFromSql(u8login.UfDbName, "select cDept_num from hr_hi_person where cPsn_Name='" + body.person + "'");
                        personcode = DBhelper.getDataFromSql(u8login.UfDbName, "select cPsn_num from hr_hi_person where cPsn_Name='" + body.person + "'");
                        if (depcode == "")
                        {
                            strResult = body.person + "在U8人员档案中不存在";
                            re.recode = "222";
                            re.remsg = strResult;
                            return;
                        }
                    }
                    xnnodeclone.attributes.getNamedItem("cDepCode").text = depcode;
                    xnnodeclone.attributes.getNamedItem("cPersonCode").text = personcode;

                    CodeResult codeDebit = ARAPCodeEntity.getAPKzkm(pay.ztcode,body.yjkm,body.ejkm, u8login,"pay");
                    if (codeDebit != null)
                    {
                        if (!string.IsNullOrEmpty(codeDebit.recode))
                        {
                            xnnodeclone.attributes.getNamedItem("cKm").text = codeDebit.recode;
                            cashitemcode = codeDebit.cashitemcode;
                        }
                        if (!string.IsNullOrEmpty(codeDebit.itemClass))
                        {
                            xnnodeclone.attributes.getNamedItem("cXmClass").text = codeDebit.itemClass;
                            //检查项目是否存在
                            if (!string.IsNullOrEmpty(body.projname))
                            {
                                //itemcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemcode from fitemss" + codeDebit.itemClass + " where citemname='" + body.projname + "'");
                                strSql="select citemcode from fitemss" + codeDebit.itemClass + " where citemcode='" + body.projname + "'";
                                LogHelper.WriteLog(typeof(PayEntity),"getItemcodeSql:"+strSql);
                                itemcode = DBhelper.getDataFromSql(u8login.UfDbName, strSql);
                                if (string.IsNullOrEmpty(itemcode))
                                {
                                    strResult = body.projname + "在U8项目管理档案中不存在";
                                    re.recode = "222";
                                    re.remsg = strResult;
                                    return;
                                }
                                xnnodeclone.attributes.getNamedItem("cXm").text = itemcode;
                            }
                           
                        }
                    }
                    
                    xnnodeclone.attributes.getNamedItem("cDigest").text = body.memo;
                    xnnodeclone.attributes.getNamedItem("cMemo").text = body.memo;
                    xnnodeclone.attributes.getNamedItem("cDefine22").text = pay.head.oacode;
                    xnnodeclone.attributes.getNamedItem("cDefine23").text = body.person;
                    xnnodeclone.attributes.getNamedItem("cDefine24").text =body.yjkm;
                    xnnodeclone.attributes.getNamedItem("cDefine25").text = body.ejkm;
                    //现金流量
                    xnnodeclone.attributes.getNamedItem("cDefine28").text = cashitemcode;
                    domBody.selectSingleNode("xml").selectSingleNode("rs:data").appendChild(xnnodeclone);

                }
                if (domBody.selectSingleNode("xml").selectSingleNode("rs:data").childNodes.length > 1)
                {
                    domBody.selectSingleNode("xml").selectSingleNode("rs:data").removeChild(xnnode);
                }
                #endregion
                //LogHelper.WriteLog(typeof(PayEntity), "xml head start");
                #region//head
                MSXML2.IXMLDOMNode xnnodehead = domHead.selectSingleNode("xml").selectSingleNode("rs:data").selectSingleNode("z:row");
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
                xnnodehead.attributes.getNamedItem("cDwCode").text = vencode;
                xnnodehead.attributes.getNamedItem("cDeptCode").text = depcode;
                xnnodehead.attributes.getNamedItem("cPerson").text = personcode;
                CodeResult codeCredit = ExpenseCodeEntity.getAccountCode(pay.ztcode, pay.head.accountcode);
                if (codeCredit!=null)
                {
                    if (!string.IsNullOrEmpty(codeCredit.recode))
                    {
                        xnnodehead.attributes.getNamedItem("cCode").text = codeCredit.recode;
                    }
                }
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

                //LogHelper.WriteLog(typeof(PayEntity), "netcwapi start");
                /*
                NetCWAPI.U8NetCWAPIClass uncw = new NetCWAPI.U8NetCWAPIClass();
                uncw.NetCWToVBAdd(u8login.userToken, conn, "SaveVouch", "付款单", domHead.xml, domBody.xml, ref bTran, ref strResult);
                */
                UFAPBO.clsVouchFacade myVouch = new UFAPBO.clsVouchFacade();
                object oSource = Type.Missing;
                myVouch.Init("收款单", u8login, conn, "AP");
                bTran = true;
                bTran = myVouch.SaveVouch(domHead, domBody, ref strResult, bTran);
                //LogHelper.WriteLog(typeof(PayEntity), "netcwapi result");
                //if (string.IsNullOrEmpty(strResult))
                if (bTran)
                {
                    re.recode = "0";
                    re.remsg = "";
                    vouchID = DBhelper.getDataFromSql(u8login.UfDbName, "select cvouchid from ap_closebill where cdefine11='" + pay.head.oacode + "' and cVouchType='49'");
                    cLink = DBhelper.getDataFromSql(u8login.UfDbName, "select iID from ap_closebill where cdefine11='" + pay.head.oacode + "' and cVouchType='49'");
                    DBhelper.setDataFromSql(u8login.UfDbName, "update ap_closebill set iperiod=" + pay.head.ddate.Month.ToString() + " where iID=" + cLink + " and cVouchType='49'");
                    re.u8code = vouchID;
                    re.u8code = vouchID;
                    #region//verify vouch
                    bTran = Verify(u8login, conn, vouchID, cLink);

                    if (bTran)
                    {
                        bTran = MakeVouch(u8login, conn, vouchID, cLink, pay.head.ddate,pay);
                    }
                    #endregion
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
                LogHelper.WriteLog(typeof(PayEntity), ex);
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
        /*
        * 20220329自动审核单据
        */
        private static bool Verify(U8Login.clsLogin u8login, ADODB.Connection conn, string vouchID, string cLink)
        {
            bool bTran = true;
            string strResult = "";
            UFAPBO.clsCloseBill myClsVouch1 = new UFAPBO.clsCloseBill();
            //UFAPBO.clsAPVouch myClsVouch1 = new UFAPBO.clsAPVouch();
            myClsVouch1.Init(u8login, conn);//, "AR");
            MSXML2.DOMDocument40 oDom = new MSXML2.DOMDocument40();
            oDom.loadXML("<condition/>");
            MSXML2.IXMLDOMElement oElm = oDom.documentElement;
            //oElm.setAttribute("cLinkFld","iID");
            oElm.setAttribute("iID", cLink);
            oElm.setAttribute("cVouchID", vouchID);
            oElm.setAttribute("cVouchType", "49");
            oElm.setAttribute("type", 0);
            oElm.setAttribute("bFirst", false);
            oElm.setAttribute("cFlag", "AP");
            bTran = myClsVouch1.Sign(oElm.xml, ref strResult);
            if (!bTran)
            {
                LogHelper.WriteLog(typeof(PayEntity), "verify error:" + strResult);
            }

            return bTran;
        }
        /*
         * 20200329 生成凭证
         */
        private static bool MakeVouch(U8Login.clsLogin u8login, ADODB.Connection conn, string vouchID, string cLink, DateTime ddate, ClsPay pay)
        {
            bool bTran = true;
            string strGuid = Guid.NewGuid().ToString("N");
            string strSql = "";
            string Year = "";//年
            string Month = "";//月
            string Date = "";//日
            object objOut = null;
            string ccode = "";
            string cmaker = "";
            List<CashFlow> cashFlows = new List<CashFlow>();
            XmlDocument xmlDocCashFlow = new XmlDocument();
            xmlDocCashFlow.Load(HttpContext.Current.Server.MapPath("..") + "\\UFIDA\\CashFlowColXml.xml");
            //20220605 制单人等于推单人
            if (string.IsNullOrEmpty(pay.head.cmaker))
            {
                cmaker = u8login.cUserName;
            }
            else
            {
                cmaker = pay.head.cmaker;
            }

            //2 创建组件
            CVoucher.CVInterface obj = new CVoucher.CVInterface();
            //3 创建临时表
            Date = Convert.ToDateTime(ddate).ToShortDateString();
            Month = Convert.ToDateTime(ddate).Month.ToString();
            Year = Convert.ToDateTime(ddate).Year.ToString();
            obj.strTempTable = "tempdb..cus_gl_accvouch_" + strGuid;
            strSql = "CREATE TABLE " + obj.strTempTable + " "
                    + "(csign nvarchar(28),ino_id smallint, "
                    + "inid smallint, cbill nvarchar(80), doutbilldate DATETIME, ccashier nvarchar(80), "
                    + "idoc smallint default 0, ctext1 nvarchar(50), ctext2 nvarchar(50), cexch_name nvarchar(28), "
                    + "cdigest nvarchar(120), ccode nvarchar(40), md money default 0, mc money default 0, "
                    + "md_f money default 0, mc_f money default 0, nfrat float default 0, nd_s float default 0, nc_s float default 0, csettle nvarchar(23), "
                    + "cn_id nvarchar(30), dt_date DATETIME, cdept_id nvarchar(12), cperson_id nvarchar(80), csup_id nvarchar(80), ccus_id nvarchar (20), "
                    + "citem_id nvarchar(80), citem_class nvarchar(22), cname nvarchar(40), ccode_equal nvarchar(50), "
                    + "bvouchedit bit default 1, bvouchaddordele bit default 1, bvouchmoneyhold bit default 1, bvalueedit bit default 1, bcodeedit bit default 1, ccodecontrol nvarchar(50), bPCSedit bit default 1, bDeptedit bit default 1, bItemedit bit default 1, bCusSupInput bit default 1, " 
                    + "coutaccset nvarchar(23), ioutyear smallint, coutsysname nvarchar(50) NOT NULL, coutsysver nvarchar(50), ioutperiod tinyint NOT NULL, coutsign nvarchar(80) NOT NULL, coutno_id nvarchar(100) NOT NULL, doutdate DATETIME, coutbillsign nvarchar(80), coutid nvarchar(50), iflag tinyint"
                    + ",iBG_ControlResult smallint null,daudit_date DateTime NULL,cblueoutno_id nvarchar(50) NULL,bWH_BgFlag bit,cDefine1 nvarchar(40),"
                    + "cDefine2 nvarchar(40),cDefine3 nvarchar(40),cDefine4 DateTime,cDefine5 int,cDefine6 DateTime,cDefine7 Float,cDefine8 nvarchar(4),cDefine9 nvarchar(8),"
                    + "cDefine10 nvarchar(60),cDefine11 nvarchar(120),cDefine12 nvarchar(120),cDefine13 nvarchar(120),cDefine14 nvarchar(120),cDefine15 int,cDefine16 float)";
            conn.Execute(strSql, out objOut);
            int iRow = 1;

            String cDigest ="付"+ pay.head.vendor + "采购款 " + pay.head.oacode;

            DataTable dtHead = DBhelper.getDatatableFromSql(u8login.UfDbName, "select * from ap_closebill where iID='" + cLink + "'");
            DataRow drHead = dtHead.Rows[0];

            #region//借方
            DataTable dtBody = DBhelper.getDatatableFromSql(u8login.UfDbName, "select * from ap_closebills where iID='" + cLink + "'");
            String ccodeDebit = "";
           
            foreach (DataRow drBody in dtBody.Rows)
            {
               ccodeDebit = "";
                //无税金额
               cDigest = "付" + pay.head.vendor+" " + drBody["cDefine25"].ToString() + "采购款 " + pay.head.oacode;
                strSql = "insert into " + obj.strTempTable
                + "(ioutperiod,csup_id,coutbillsign,coutid,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,md,cdept_id,citem_id,citem_class,cperson_id)  values("
                + Month + ",'" + drHead["cDwCode"].ToString() + "','49','49" + vouchID + "','记','记','" + cDigest + "','AP49" + vouchID + "','AP','" + cmaker + "'," + iRow.ToString() + ",'" + drBody["cKm"].ToString() + "',null,'" + ddate.ToShortDateString() + "',null,1,1,1," + drBody["iAmt"].ToString()
                + ",'" + drBody["cDepCode"].ToString() + "','"+drBody["cXm"].ToString()+"','"+drBody["cXmClass"].ToString()+"','" + drBody["cPersonCode"].ToString() + "')";
                LogHelper.WriteLog(typeof(PayEntity), "glaccvouch:" + strSql);
                conn.Execute(strSql, out objOut);
                //现金流量
                CashFlow cashFlow = new CashFlow();
                cashFlow.Amount_f =Convert.ToDecimal(drBody["iAmt"]);
                cashFlow.Amount = Convert.ToDecimal(drBody["iAmt"]);
                cashFlow.cCashItem = drBody["cDefine28"].ToString();
                cashFlows.Add(cashFlow);

                iRow++;
               
            }
            #endregion

            #region//贷方
            
            //无税金额
            //ccode = "1002010101";
            string ccodeCredit = "";           

            strSql = "insert into " + obj.strTempTable
            + "(ioutperiod,csup_id,coutbillsign,coutid,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,mc,cdept_id,citem_id,citem_class,cperson_id)  values("
            + Month + ",'" + drHead["cDwCode"].ToString() + "','49','49" + vouchID + "','记','记','" + cDigest + "','AP49" + vouchID + "','AP','" + cmaker + "'," + iRow.ToString() + ",'" + drHead["cCode"].ToString() + "',null,'" + ddate.ToShortDateString() + "',null,1,1,1," + drHead["iAmount"].ToString()
            + ",'" + drHead["cDeptCode"].ToString() + "','" + drHead["cItemCode"].ToString() + "','" + drHead["cItem_Class"].ToString() + "','" + drHead["cPerson"].ToString() + "')";
            LogHelper.WriteLog(typeof(PayEntity), "glaccvouch:" + strSql);
            conn.Execute(strSql, out objOut);

            //现金流量
            setCashXml(u8login.UfDbName, obj, xmlDocCashFlow,
                                  "AP" + vouchID, Month, Year, Date,
                                 cDigest, "", "",
                                  "", "", "", "",
                                  drHead["cCode"].ToString(), cashFlows, iRow.ToString(), "credit");

            iRow++;
            #endregion
            obj.set_Connection(conn);
            obj.LoginByUserToken(u8login.userToken);
            obj.CashFlowColXml = xmlDocCashFlow.OuterXml;
            bTran = obj.SaveVoucher();
            if (bTran)
            {
                String pzID = DBhelper.getDataFromSql(u8login.UfDbName, "select ino_id result from gl_accvouch where coutbillsign='49' and coutid='49" + vouchID + "'");
                String cpzID = DBhelper.getDataFromSql(u8login.UfDbName, "select coutno_id result from gl_accvouch where coutbillsign='49' and coutid='49" + vouchID + "'");
                String cPzNum = "记-" + string.Format("{0:D4}", Convert.ToInt32(pzID));

                strSql = "update ap_closebill set cPzId='" + cpzID + "',cPzNum='" + cPzNum + "',doutbilldate='" + ddate.ToShortDateString() + "' where iID='" + cLink + "'";
                LogHelper.WriteLog(typeof(PayEntity), "apvouch:" + strSql);
                DBhelper.setDataFromSql(u8login.UfDbName, strSql);

                strSql = "update Ap_CloseBill "
                    + "set cPzID='" + cpzID + "',cPZNum='" + cPzNum + "',doutbilldate='" + ddate.ToShortDateString() + "'"
                        + " where cVouchID='" + vouchID + "' and cVouchType='49'";
                LogHelper.WriteLog(typeof(PayEntity), "apvouch:" + strSql);
                DBhelper.setDataFromSql(u8login.UfDbName, strSql);

                //strSql = " update Ap_Detail set ccode='" + ccodeDebit+ "',isignseq=1,cglsign='记',iglno_id=" + pzID + ",ino_id=" + pzID + ",cDigest='付款单',cPZid='" + cpzID + "' where cVouchID='" + vouchID + "' and cVouchType='49' and cCoVouchID='" + vouchID + "' and cCoVouchType='49' and iflag=0";
                strSql = "update Ap_Detail set cCode=b.cCode,isignseq=1,cglsign='记',iglno_id=" + pzID + ",ino_id=" + pzID + ",cDigest='" + cDigest + "',cPZid='" + cpzID + "'"
                        + " from Ap_Detail a inner join Ap_CloseBill b"
                        + " on a.cVouchID=b.cVouchID and a.cCoVouchID=b.cVouchID "
                        + " and iClosesID=0 and iCoClosesID=0"
                        + " and a.cVouchType=b.cVouchType and a.cCoVouchType=b.cVouchType"
                        + " and a.cVouchType='49' and a.cCoVouchType='49'"
                        + " and a.cVouchID='" + vouchID + "' and a.cCoVouchID='" + vouchID + "'";               
                LogHelper.WriteLog(typeof(PayEntity), "apvouch:" + strSql);
                DBhelper.setDataFromSql(u8login.UfDbName, strSql);

                //strSql = " update Ap_Detail set ccode='" + ccodeCredit + "',isignseq=1,cglsign='记',iglno_id=" + pzID + ",ino_id=" + pzID + ",cDigest='付款单',cPZid='" + cpzID + "' where cVouchID='" + vouchID + "' and cVouchType='49' and cCoVouchID='" + vouchID + "' and cCoVouchType='49' and iflag!=0";
                strSql = "update Ap_Detail set cCode=b.cKm,isignseq=1,cglsign='记',iglno_id=" + pzID + ",ino_id=" + pzID + ",cDigest='"+cDigest+"',cPZid='" + cpzID + "'"
                        +" from Ap_Detail a inner join Ap_CloseBills b" 
                        +" on iClosesID=b.ID and iCoClosesID=b.ID"
                        + " and a.cVouchType='49' and a.cCoVouchType='49'"
                        + " and a.cVouchID='" + vouchID + "' and a.cCoVouchID='" + vouchID + "'";
                LogHelper.WriteLog(typeof(PayEntity), "apvouch:" + strSql);
                DBhelper.setDataFromSql(u8login.UfDbName, strSql);
            }
            else
            {
                LogHelper.WriteLog(typeof(PayEntity), "apvouch:" + obj.strErrMessage.ToString());
            }

            return bTran;
        }
        private static void setCashXml(string ConnStr, CVoucher.CVInterface obj, XmlDocument xmlDocCashFlow, string strGuid, string Month, string Year, string Date, string Digest, string EXCH, string VEN, string Dep, string PER, string ItemClass, string Item, string CCODE, List<CashFlow> cashFlows, string Inid, string Debit)
        {
            XmlNode xmlNo = xmlDocCashFlow.SelectSingleNode("root/rows").FirstChild;
            int iRow = 1;

            if (DBhelper.getDataFromSql(ConnStr, "select ccode from code where bCashItem=1 and ccode='" + CCODE + "' and iyear=" + Year) != "")
            {
                foreach (CashFlow cashFlow in cashFlows)
                {
                    XmlNode xnClone = xmlNo.Clone();
                    xnClone.Attributes["key"].Value = Guid.NewGuid().ToString();
                    xnClone.Attributes["RowGuid"].Value = strGuid;// xnClone.Attributes["key"].Value;
                    xnClone.Attributes["iPeriod"].Value = Month;
                    xnClone.Attributes["iYear"].Value = Year;
                    xnClone.Attributes["iYPeriod"].Value = Year + string.Format("{0:D2}", Convert.ToInt32(Month));
                    xnClone.Attributes["cCashItem"].Value = cashFlow.cCashItem;
                    xnClone.Attributes["ccode"].Value = CCODE;
                    xnClone.Attributes["dbill_date"].Value = Date;
                    xnClone.Attributes["cdigest"].Value = Digest;
                    xnClone.Attributes["iRow"].Value = iRow.ToString();
                    xnClone.Attributes["inid"].Value = Inid;
                    xnClone.Attributes["cexch_name"].Value = "";
                    xnClone.Attributes["csup_id"].Value = VEN;
                    xnClone.Attributes["cdept_id"].Value = Dep;
                    xnClone.Attributes["cperson_id"].Value = PER;
                    xnClone.Attributes["citem_class"].Value = ItemClass;
                    xnClone.Attributes["citem_id"].Value = Item;

                    switch (Debit.ToLower())
                    {
                        case "debit":
                            xnClone.Attributes["md_f"].Value = (cashFlow.Amount_f).ToString();
                            xnClone.Attributes["md"].Value = (cashFlow.Amount).ToString();
                            xnClone.Attributes["mc_f"].Value = "0";
                            xnClone.Attributes["mc"].Value = "0";
                            break;
                        case "credit":
                            xnClone.Attributes["mc_f"].Value = (cashFlow.Amount_f).ToString();
                            xnClone.Attributes["mc"].Value = (cashFlow.Amount).ToString();
                            xnClone.Attributes["md_f"].Value = "0";
                            xnClone.Attributes["md"].Value = "0";
                            break;
                    }

                    iRow++;

                    xmlDocCashFlow.SelectSingleNode("root/rows").AppendChild(xnClone);
                }


            }
            xmlDocCashFlow.SelectSingleNode("root/rows").RemoveChild(xmlNo);
            //xmlDocCashFlow.Save(HttpContext.Current.Server.MapPath("..") + "\\Logs\\xmlDocCashFlow.xml");
        }    
    }
}