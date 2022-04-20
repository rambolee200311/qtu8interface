using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QTU8interface.UFIDA;
using QTU8interface.Models.Payable;
using QTU8interface.Models.Result;
using System.Data;
namespace QTU8interface.Entities
{
    public class PayableEntity
    {
        public static void Add_Payable(ClsPayable payable,ref ClsResult re)
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
            bool bTran = false;
            MSXML2.IXMLDOMDocument2 domHead = new MSXML2.DOMDocument30Class();
            MSXML2.IXMLDOMDocument2 domBody = new MSXML2.DOMDocument30Class();

            U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity(payable.ztcode.ToString(), payable.head.ddate.Year.ToString(), payable.head.ddate.ToShortDateString());
            if (u8login == null)
            {
                re.recode = "111";
                re.remsg = re.ztcode + "对应帐套登录失败";
                return;
            }
            //检查应付单是否重复
            strResult = DBhelper.getDataFromSql(u8login.UfDbName, "select cdefine11 from ap_vouch where cdefine11='" + payable.head.oacode + "'");
            if (strResult != "")
            {
                strResult = payable.head.oacode + "已生成过应付单";
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
                itemcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemcode from fitemss97 where citemcode='" + payable.head.projname + "'");
                if (itemcode == "")
                {
                    strResult = payable.head.projname + "在U8项目管理档案中不存在";
                    re.recode = "222";
                    re.remsg = strResult;
                    return;
                }
            }
            //检查供应商是否存在
            vencode = DBhelper.getDataFromSql(u8login.UfDbName, "select cvencode from vendor where cvenname='" + payable.head.vendor + "'");
            if (vencode == "")
            {
                strResult = payable.head.vendor + "在U8供应商档案中不存在";
                re.recode = "222";
                re.remsg = strResult;
                return;
            }
            try
            {
                conn.Open(u8login.UfDbName);
                domHead.load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\APVouchHeadADD.xml");
                domBody.load(AppDomain.CurrentDomain.BaseDirectory + "UFIDA\\APVouchBodyADD.xml");



                #region//body
                MSXML2.IXMLDOMNode xnnode = domBody.selectSingleNode("xml").selectSingleNode("rs:data").selectSingleNode("z:row");
                sumamount = 0m;
                foreach (Payable_Body body in payable.body)
                {
                    /*                    
                    tax = body.tax;                    
                    if (money != 0)
                    { taxrate = (tax / money) * 100; }
                    
                     * */
                    amount = body.amount;
                    tax = 0;
                    money = amount - tax;
                    taxrate = 0;
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
                    //xnnodeclone.attributes.getNamedItem("cItemCode").text = itemcode;
                    //xnnodeclone.attributes.getNamedItem("cItem_Class").text = "97";
                    xnnodeclone.attributes.getNamedItem("cDigest").text = body.memo;

                    
                    
                    string yjkm = "";
                    string ejkm = "";
                    string sajkm = "";
                    string sijkm = "";
                    string person = "";
                    if (!string.IsNullOrEmpty(body.yjkm))
                    {
                        yjkm = body.yjkm;
                    }
                    if (!string.IsNullOrEmpty(body.ejkm))
                    {
                        ejkm = body.ejkm;
                    }
                    if (!string.IsNullOrEmpty(body.sajkm))
                    {
                        sajkm = body.sajkm;
                    }
                    if (!string.IsNullOrEmpty(body.sijkm))
                    {
                        sijkm = body.sijkm;
                    }
                    if (!string.IsNullOrEmpty(body.person))
                    {
                        person = body.person;
                    }
                    xnnodeclone.attributes.getNamedItem("cDefine22").text = yjkm;
                    xnnodeclone.attributes.getNamedItem("cDefine23").text = ejkm;
                    xnnodeclone.attributes.getNamedItem("cDefine24").text = sajkm;
                    xnnodeclone.attributes.getNamedItem("cDefine25").text = sijkm;
                    xnnodeclone.attributes.getNamedItem("cDefine28").text = person;
                    CodeResult codeDebit=null;
                    if (body.yjkm.ToString() != "应交税费")
                    {
                        codeDebit = ARAPCodeEntity.getAPCbfykm(payable.ztcode,yjkm, ejkm,sajkm,sijkm,person, u8login);
                    }
                    else
                    {
                        codeDebit = ARAPCodeEntity.getCgsjkm(payable.ztcode);
                    }
                   
                    if (codeDebit != null)
                    {
                        if (!string.IsNullOrEmpty(codeDebit.recode))
                        {
                            xnnodeclone.attributes.getNamedItem("cCode").text = codeDebit.recode;
                            if (!string.IsNullOrEmpty(codeDebit.midcode))
                            { xnnodeclone.attributes.getNamedItem("cDefine27").text = codeDebit.midcode; }
                            else
                            { xnnodeclone.attributes.getNamedItem("cDefine27").text = ""; }
                        }
                        if (!string.IsNullOrEmpty(codeDebit.itemClass))
                        {
                            xnnodeclone.attributes.getNamedItem("cItem_Class").text = codeDebit.itemClass;
                            //检查项目是否存在
                            if (!string.IsNullOrEmpty(body.projname))
                            {
                                itemcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemcode from fitemss" + codeDebit.itemClass + " where citemcode='" + body.projname + "'");
                                if (string.IsNullOrEmpty(itemcode))
                                {
                                    strResult = body.projname + "在U8项目管理档案中不存在";
                                    re.recode = "222";
                                    re.remsg = strResult;
                                    return;
                                }
                                xnnodeclone.attributes.getNamedItem("cItemCode").text = itemcode;
                                xnnodeclone.attributes.getNamedItem("cItemName").text = body.projname;
                               
                            }

                        }
                    }

                    domBody.selectSingleNode("xml").selectSingleNode("rs:data").appendChild(xnnodeclone);

                }
                if (domBody.selectSingleNode("xml").selectSingleNode("rs:data").childNodes.length>1)
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

                string yjkm1 = "";
                string ejkm1 = "";
                if (!string.IsNullOrEmpty(payable.head.yjkm))
                {
                    yjkm1 = payable.head.yjkm;
                }
                if (!string.IsNullOrEmpty(payable.head.ejkm))
                {
                    ejkm1 = payable.head.ejkm;
                }
                CodeResult codeCredit = ARAPCodeEntity.getAPKzkm(payable.ztcode, yjkm1, ejkm1, u8login,"payable");
                if (codeCredit != null)
                {
                    if (!string.IsNullOrEmpty(codeCredit.recode))
                    {
                        xnnodehead.attributes.getNamedItem("cCode").text = codeCredit.recode;
                    }
                    if (!string.IsNullOrEmpty(codeCredit.itemClass))
                    {
                        xnnodehead.attributes.getNamedItem("cItem_Class").text = codeCredit.itemClass;
                        //检查项目是否存在
                        if (!string.IsNullOrEmpty(payable.head.projname))
                        {
                            itemcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemcode from fitemss" + codeCredit.itemClass + " where citemcode='" + payable.head.projname + "'");
                            if (string.IsNullOrEmpty(itemcode))
                            {
                                strResult = payable.head.projname + "在U8项目管理档案中不存在";
                                re.recode = "222";
                                re.remsg = strResult;
                                return;
                            }
                            xnnodehead.attributes.getNamedItem("cItemCode").text = itemcode;
                            xnnodehead.attributes.getNamedItem("cItemName").text = payable.head.projname;
                        }

                    }
                 }

                #endregion
                /*
                NetCWAPI.U8NetCWAPIClass uncw = new NetCWAPI.U8NetCWAPIClass();
                uncw.NetCWToVBAdd(u8login.userToken, conn, "SaveVouch", "应付单", domHead.xml, domBody.xml, ref bTran, ref strResult);
                */
                UFAPBO.clsVouchFacade myVouch = new UFAPBO.clsVouchFacade();
                object oSource = Type.Missing;
                myVouch.Init("应付单", u8login, conn, "AP");
                bTran = true;
                bTran = myVouch.SaveVouch(domHead, domBody, ref strResult, bTran);
                //if (string.IsNullOrEmpty(strResult))
                if (bTran)
                {
                    re.recode = "0";
                    re.remsg = "";
                    vouchID = DBhelper.getDataFromSql(u8login.UfDbName, "select cvouchid from ap_vouch where cdefine11='" + payable.head.oacode + "' and cvouchtype='P0'");
                    cLink = DBhelper.getDataFromSql(u8login.UfDbName, "select cLink from ap_vouch where cdefine11='" + payable.head.oacode + "' and cvouchtype='P0'");
                    re.u8code =vouchID;

                    #region//verify vouch
                    bTran = Verify(u8login, conn, vouchID, cLink);
                    if (bTran)
                    {
                        bTran = MakeVouch(u8login, conn, vouchID, cLink, payable.head.ddate,payable);
                    }
                    #endregion
                    return;
                }
                else
                {
                    re.recode = "999";
                    re.remsg = strResult;
                    LogHelper.WriteLog(typeof(PayableEntity), "add error:" + strResult);
                    return;
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(typeof(PayableEntity),ex);
                re.recode = "999";
                re.remsg = ex.Message;
                return;
            }
            finally
            {
                if (conn.State==1)
                {
                    conn.Close();
                }
            }
        }
        /*
         * 20220328 自动审核单据
         */
        private static bool Verify(U8Login.clsLogin u8login, ADODB.Connection conn, string vouchID, string cLink)
        {
            bool bTran = true;
            string strResult = "";
            UFAPBO.clsAPVouch myClsVouch1 = new UFAPBO.clsAPVouch();
            myClsVouch1.Init(u8login, conn, "AP");
            MSXML2.DOMDocument40 oDom = new MSXML2.DOMDocument40();
            oDom.loadXML("<condition/>");
            MSXML2.IXMLDOMElement oElm = oDom.documentElement;
            oElm.setAttribute("cLink", cLink);
            oElm.setAttribute("cVouchID", vouchID);
            oElm.setAttribute("cVouchType", "P0");
            oElm.setAttribute("type", 0);
            oElm.setAttribute("bFirst", false);
            bTran = myClsVouch1.Sign(oElm.xml, ref strResult);
            if (!bTran)
            {
                LogHelper.WriteLog(typeof(PayableEntity), "verify error:" + strResult);
            }

            return bTran;
        }
        /*
         * 20200328 生成凭证
         */
        private static bool MakeVouch(U8Login.clsLogin u8login, ADODB.Connection conn, string vouchID, string cLink, DateTime ddate,ClsPayable payable)
        {
            bool bTran = true;
            string strGuid = Guid.NewGuid().ToString("N");
            string strSql = "";
            string Year = "";//年
            string Month = "";//月
            string Date = "";//日
            object objOut = null;
            string ccode = "";
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
                    + "cn_id nvarchar(30), dt_date DATETIME, cdept_id nvarchar(12), cperson_id nvarchar(80), ccus_id nvarchar(80), csup_id nvarchar (20), "
                    + "citem_id nvarchar(80), citem_class nvarchar(22), cname nvarchar(40), ccode_equal nvarchar(50), "
                    + "bvouchedit bit default 0, bvouchaddordele bit default 0, bvouchmoneyhold bit default 0, bvalueedit bit default 0, bcodeedit bit default 0, ccodecontrol nvarchar(50), bPCSedit bit default 0, bDeptedit bit default 0, bItemedit bit default 0, bCusSupInput bit default 0, "
                    + "coutaccset nvarchar(23), ioutyear smallint, coutsysname nvarchar(50) NOT NULL, coutsysver nvarchar(50), ioutperiod tinyint NOT NULL, coutsign nvarchar(80) NOT NULL, coutno_id nvarchar(100) NOT NULL, doutdate DATETIME, coutbillsign nvarchar(80), coutid nvarchar(50), iflag tinyint"
                    + ",iBG_ControlResult smallint null,daudit_date DateTime NULL,cblueoutno_id nvarchar(50) NULL,bWH_BgFlag bit,cDefine1 nvarchar(40),"
                    + "cDefine2 nvarchar(40),cDefine3 nvarchar(40),cDefine4 DateTime,cDefine5 int,cDefine6 DateTime,cDefine7 Float,cDefine8 nvarchar(4),cDefine9 nvarchar(8),"
                    + "cDefine10 nvarchar(60),cDefine11 nvarchar(120),cDefine12 nvarchar(120),cDefine13 nvarchar(120),cDefine14 nvarchar(120),cDefine15 int,cDefine16 float)";
            conn.Execute(strSql, out objOut);
            int iRow = 1;

            DataTable dtHead = DBhelper.getDatatableFromSql(u8login.UfDbName, "select * from ap_vouch where clink='" + cLink + "'");
            DataRow drHead = dtHead.Rows[0];
            #region//借方
            DataTable dtBody = DBhelper.getDatatableFromSql(u8login.UfDbName, "select * from ap_vouchs where clink='" + cLink + "'");
            string cDigest = "收" + payable.head.vendor + " " + payable.head.ejkm + " 回票 " + payable.head.oacode;
            foreach (DataRow drBody in dtBody.Rows)
            {
                //无税金额
                
                strSql = "insert into " + obj.strTempTable
                + "(ioutperiod,csup_id,coutbillsign,coutid,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,md,cdept_id,citem_id,citem_class,cperson_id)  values("
                + Month + ",'" + drHead["cDwCode"].ToString() + "','P0','" + vouchID + "','记','记','"+cDigest+"','AP" + vouchID + "','AP','" + u8login.cUserName + "'," + iRow.ToString() + ",'" + drBody["cCode"].ToString() + "',null,'" + ddate.ToShortDateString() + "',null,1,1,1," + drBody["iNoTaxAmount"].ToString()
                + ",'" + drBody["cDeptCode"].ToString() + "','" + drBody["cItemCode"].ToString() + "','" + drBody["cItem_Class"].ToString() + "','" + drBody["cPerson"].ToString() + "')";
                LogHelper.WriteLog(typeof(PayableEntity), "glaccvouch:" + strSql);
                conn.Execute(strSql, out objOut);
                iRow++;

                #region//中转科目
                if (!string.IsNullOrEmpty(drBody["cDefine27"].ToString()))
                {
                    strSql = "insert into " + obj.strTempTable
                    + "(ioutperiod,csup_id,coutbillsign,coutid,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,md,cdept_id,citem_id,citem_class,cperson_id)  values("
                    + Month + ",'" + drHead["cDwCode"].ToString() + "','P0','" + vouchID + "','记','记','" + cDigest + "','AP" + vouchID + "','AP','" + u8login.cUserName + "'," + iRow.ToString() + ",'" + drBody["cDefine27"].ToString() + "',null,'" + ddate.ToShortDateString() + "',null,1,1,1," + drBody["iNoTaxAmount"].ToString()
                    + ",'" + drBody["cDeptCode"].ToString() + "','" + drBody["cItemCode"].ToString() + "','" + drBody["cItem_Class"].ToString() + "','" + drBody["cPerson"].ToString() + "')";
                    LogHelper.WriteLog(typeof(PayableEntity), "glaccvouch:" + strSql);
                    conn.Execute(strSql, out objOut);
                    iRow++;

                    strSql = "insert into " + obj.strTempTable
                    + "(ioutperiod,csup_id,coutbillsign,coutid,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,mc,cdept_id,citem_id,citem_class,cperson_id)  values("
                    + Month + ",'" + drHead["cDwCode"].ToString() + "','P0','" + vouchID + "','记','记','" + cDigest + "','AP" + vouchID + "','AP','" + u8login.cUserName + "'," + iRow.ToString() + ",'" + drBody["cDefine27"].ToString() + "',null,'" + ddate.ToShortDateString() + "',null,1,1,1," + drBody["iNoTaxAmount"].ToString()
                    + ",'" + drBody["cDeptCode"].ToString() + "','" + drBody["cItemCode"].ToString() + "','" + drBody["cItem_Class"].ToString() + "','" + drBody["cPerson"].ToString() + "')";
                    LogHelper.WriteLog(typeof(PayableEntity), "glaccvouch:" + strSql);
                    conn.Execute(strSql, out objOut);
                    iRow++;
                }

                #endregion
                //税额
                /*
                if (Convert.ToDecimal(drBody["iTax"]) != 0)
                {
                    ccode = "2221010101";
                    CodeResult codeCgsjkm = ARAPCodeEntity.getCgsjkm(payable.ztcode);
                    if (codeCgsjkm!=null)
                    {
                        if (!string.IsNullOrEmpty(codeCgsjkm.recode))
                        { ccode = codeCgsjkm.recode; }
                    }
                    strSql = "insert into " + obj.strTempTable
                    + "(ioutperiod,csup_id,coutbillsign,coutid,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,md,cdept_id,citem_id,citem_class,cperson_id)  values("
                    + Month + ",'" + drHead["cDwCode"].ToString() + "','P0','" + vouchID + "','记','记','" + cDigest + "','AP" + vouchID + "','AP','" + u8login.cUserName + "'," + iRow.ToString() + ",'" + ccode + "',null,'" + ddate.ToShortDateString() + "',null,1,1,1," + drBody["iTax"].ToString()
                    + ",'" + drBody["cDeptCode"].ToString() + "','" + drBody["cItemCode"].ToString() + "','" + drBody["cItem_Class"].ToString() + "','" + drBody["cPerson"].ToString() + "')";
                    LogHelper.WriteLog(typeof(PayableEntity), "glaccvouch:" + strSql);
                    conn.Execute(strSql, out objOut);
                    iRow++;
                }
                */
            }
            #endregion

            #region//贷方
           
            //无税金额
            ccode = drHead["cCode"].ToString();
            strSql = "insert into " + obj.strTempTable
            + "(ioutperiod,csup_id,coutbillsign,coutid,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,mc,cdept_id,citem_id,citem_class,cperson_id)  values("
            + Month + ",'" + drHead["cDwCode"].ToString() + "','P0','" + vouchID + "','记','记','" + cDigest + "','AP" + vouchID + "','AP','" + u8login.cUserName + "'," + iRow.ToString() + ",'" + ccode + "',null,'" + ddate.ToShortDateString() + "',null,1,1,1," + drHead["iAmount"].ToString()
            + ",'" + drHead["cDeptCode"].ToString() + "','" + drHead["cItemCode"].ToString() + "','" + drHead["cItem_Class"].ToString() + "','" + drHead["cPerson"].ToString() + "')";
            LogHelper.WriteLog(typeof(PayableEntity), "glaccvouch:" + strSql);
            conn.Execute(strSql, out objOut);
            iRow++;
            #endregion

            obj.set_Connection(conn);
            obj.LoginByUserToken(u8login.userToken);
            bTran = obj.SaveVoucher();
            if (bTran)
            {
                String pzID = DBhelper.getDataFromSql(u8login.UfDbName, "select ino_id result from gl_accvouch where coutbillsign='P0' and coutid='" + vouchID + "'");
                String cpzID = DBhelper.getDataFromSql(u8login.UfDbName, "select coutno_id result from gl_accvouch where coutbillsign='P0' and coutid='" + vouchID + "'");
                String cPzNum = "记-" + string.Format("{0:D4}", Convert.ToInt32(pzID));
                strSql = "update ap_vouch set cPzId='" + cpzID + "',cPzNum='" + cPzNum + "',doubbilldate='" + ddate.ToShortDateString() + "' where cLink='" + cLink + "'";
                LogHelper.WriteLog(typeof(PayableEntity), "apvouch:" + strSql);
                DBhelper.setDataFromSql(u8login.UfDbName, strSql);

                strSql = " update Ap_Detail set isignseq=1,cglsign='记',iglno_id=" + pzID + ",ino_id=" + pzID + ",cDigest='" + cDigest + "',cPZid='" + cpzID + "' where cVouchID='" + vouchID + "' and cVouchType='P0' and cCoVouchID='" + vouchID + "' and cCoVouchType='P0'";
                LogHelper.WriteLog(typeof(PayableEntity), "apvouch:" + strSql);
                DBhelper.setDataFromSql(u8login.UfDbName, strSql);
            }
            else
            {
                LogHelper.WriteLog(typeof(PayableEntity), "apvouch:" + obj.strErrMessage.ToString());
            }

            return bTran;
        }
    }
}