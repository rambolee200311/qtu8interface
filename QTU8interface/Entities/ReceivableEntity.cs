using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QTU8interface.UFIDA;
using QTU8interface.Models.Receivable;
using QTU8interface.Models.Result;
using System.Data;

namespace QTU8interface.Entities
{
    public class ReceivableEntity
    {
        public static void Add_Receivable(ClsReceivable payable, ref ClsResult re)
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
            itemcode = payable.head.projname;

            //if (!string.IsNullOrEmpty(payable.head.projname))
            //{
            //itemcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemcode from fitemss97 where citemcode='" + payable.head.projname + "'");
            //    if (itemcode == "")
            //    {
            //        strResult = payable.head.projname + "在U8项目管理档案中不存在";
            //        re.recode = "222";
            //        re.remsg = strResult;
            //        return;
            //    }
            //}
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
                    xnnodeclone.attributes.getNamedItem("cDigest").text = body.memo;

                    if (!string.IsNullOrEmpty(body.htbh))
                    { xnnodeclone.attributes.getNamedItem("cDefine22").text = body.htbh.ToString(); }//合同编号 表体自定义项1
                    if (!string.IsNullOrEmpty(body.qykh))
                    { xnnodeclone.attributes.getNamedItem("cDefine23").text = body.qykh.ToString(); }//签约客户 表体自定义项2
                    if (!string.IsNullOrEmpty(body.zzkh))
                    { xnnodeclone.attributes.getNamedItem("cDefine24").text = body.zzkh.ToString(); }//最终客户 表体自定义项3
                    if (!string.IsNullOrEmpty(body.bhsje.ToString()))
                    { xnnodeclone.attributes.getNamedItem("cDefine26").text = body.bhsje.ToString(); }//不含税金额	表体自定义项5
                    if (!string.IsNullOrEmpty(body.sqr))
                    { xnnodeclone.attributes.getNamedItem("cDefine25").text = body.sqr.ToString(); }//申请人	表体自定义项4
                    if (!string.IsNullOrEmpty(body.ywy))
                    { xnnodeclone.attributes.getNamedItem("cDefine28").text = body.ywy.ToString(); }//业务员	表体自定义项7
                    if (!string.IsNullOrEmpty(body.lcbh))
                    { xnnodeclone.attributes.getNamedItem("cDefine29").text = body.lcbh.ToString(); }//流程编号	表体自定义项8
                    if (!string.IsNullOrEmpty(body.kplb))
                    { xnnodeclone.attributes.getNamedItem("cDefine30").text = body.kplb.ToString(); }//开票类别	表体自定义项9
                    if (!string.IsNullOrEmpty(body.kpnr))
                    { xnnodeclone.attributes.getNamedItem("cDefine31").text = body.kpnr.ToString(); }//开票内容	表体自定义项10

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
                xnnodehead.attributes.getNamedItem("cDefine13").text = payable.head.person;
                xnnodehead.attributes.getNamedItem("cDigest").text = payable.head.memo;
                xnnodehead.attributes.getNamedItem("dVouchDate").text = payable.head.ddate.ToShortDateString();
                CodeResult crKzkm = ARAPCodeEntity.getKzkm(payable.ztcode, u8login);
                string codeKzkm = "";
                if (crKzkm != null)
                {
                    if (!string.IsNullOrEmpty(crKzkm.recode))
                    {
                        xnnodehead.attributes.getNamedItem("cCode").text = crKzkm.recode;
                    }
                    if (!string.IsNullOrEmpty(crKzkm.itemClass))
                    {
                        xnnodehead.attributes.getNamedItem("cItem_Class").text = crKzkm.itemClass;
                        //检查项目是否存在
                        if (!string.IsNullOrEmpty(payable.head.projname))
                        {
                            //itemcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemcode from fitemss" + codeDebit.itemClass + " where citemname='" + body.projname + "'");
                            itemcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemcode from fitemss" + crKzkm.itemClass + " where citemcode='" + payable.head.projname + "'");
                            if (string.IsNullOrEmpty(itemcode))
                            {
                                strResult = payable.head.projname + "在U8项目管理档案中不存在";
                                re.recode = "222";
                                re.remsg = strResult;
                                return;
                            }
                            xnnodehead.attributes.getNamedItem("cItemCode").text = itemcode;
                        }
                    }
                }

                #endregion
                /*
                NetCWAPI.U8NetCWAPIClass uncw = new NetCWAPI.U8NetCWAPIClass();
                uncw.NetCWToVBAdd(u8login.userToken, conn, "SaveVouch", "应收单", domHead.xml, domBody.xml, ref bTran, ref strResult);
                */
                UFAPBO.clsVouchFacade myVouch = new UFAPBO.clsVouchFacade();
                object oSource = Type.Missing;
                myVouch.Init("应收单", u8login, conn, "AR");
                bTran = true;
                bTran = myVouch.SaveVouch(domHead, domBody, ref strResult, bTran);
                //if (string.IsNullOrEmpty(strResult))
                if (bTran)
                {
                    re.recode = "0";
                    re.remsg = "";
                    vouchID = DBhelper.getDataFromSql(u8login.UfDbName, "select cvouchid from ap_vouch where cdefine11='" + payable.head.oacode + "' and cvouchtype='R0'");
                    cLink = DBhelper.getDataFromSql(u8login.UfDbName, "select cLink from ap_vouch where cdefine11='" + payable.head.oacode + "' and cvouchtype='R0'");
                    re.u8code = vouchID;

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
                    LogHelper.WriteLog(typeof(ReceivableEntity), "add error:" + strResult);
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
        /*
         * 20220326 自动审核单据
         */
        private static bool Verify(U8Login.clsLogin u8login, ADODB.Connection conn, string vouchID, string cLink)
        {
            bool bTran = true;
            string strResult = "";
            UFAPBO.clsAPVouch myClsVouch1 = new UFAPBO.clsAPVouch();
            myClsVouch1.Init(u8login, conn, "AR");
            MSXML2.DOMDocument40 oDom = new MSXML2.DOMDocument40();
            oDom.loadXML("<condition/>");
            MSXML2.IXMLDOMElement oElm = oDom.documentElement;
            oElm.setAttribute("cLink", cLink);
            oElm.setAttribute("cVouchID", vouchID);
            oElm.setAttribute("cVouchType", "R0");
            oElm.setAttribute("type", 0);
            oElm.setAttribute("bFirst", false);
            bTran = myClsVouch1.Sign(oElm.xml, ref strResult);
            if (!bTran)
            {
                LogHelper.WriteLog(typeof(ReceivableEntity), "verify error:" + strResult);
            }

            return bTran;
        }
        /*
         * 20200326 生成凭证
         */
        private static bool MakeVouch(U8Login.clsLogin u8login, ADODB.Connection conn, string vouchID, string cLink, DateTime ddate,ClsReceivable payable)
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
            //20220605 制单人等于推单人
            if (string.IsNullOrEmpty(payable.head.cmaker))
            {
                cmaker = u8login.cUserName;
            }
            else
            {
                cmaker = payable.head.cmaker;
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
                    + "cn_id nvarchar(30), dt_date DATETIME, cdept_id nvarchar(12), cperson_id nvarchar(80), ccus_id nvarchar(80), csup_id nvarchar (20), "
                    + "citem_id nvarchar(80), citem_class nvarchar(22), cname nvarchar(40), ccode_equal nvarchar(50), "
                    + "bvouchedit bit default 1, bvouchaddordele bit default 1, bvouchmoneyhold bit default 1, bvalueedit bit default 1, bcodeedit bit default 1, ccodecontrol nvarchar(50), bPCSedit bit default 1, bDeptedit bit default 1, bItemedit bit default 1, bCusSupInput bit default 1, "
                    + "coutaccset nvarchar(23), ioutyear smallint, coutsysname nvarchar(50) NOT NULL, coutsysver nvarchar(50), ioutperiod tinyint NOT NULL, coutsign nvarchar(80) NOT NULL, coutno_id nvarchar(100) NOT NULL, doutdate DATETIME, coutbillsign nvarchar(80), coutid nvarchar(50), iflag tinyint"
                    + ",iBG_ControlResult smallint null,daudit_date DateTime NULL,cblueoutno_id nvarchar(50) NULL,bWH_BgFlag bit,cDefine1 nvarchar(40),"
                    + "cDefine2 nvarchar(40),cDefine3 nvarchar(40),cDefine4 DateTime,cDefine5 int,cDefine6 DateTime,cDefine7 Float,cDefine8 nvarchar(4),cDefine9 nvarchar(8),"
                    + "cDefine10 nvarchar(60),cDefine11 nvarchar(120),cDefine12 nvarchar(120),cDefine13 nvarchar(120),cDefine14 nvarchar(120),cDefine15 int,cDefine16 float)";
            conn.Execute(strSql, out objOut);
            int iRow = 1;

            String cDigest = payable.head.customer + "开票 " + payable.head.oacode;
            DataTable dtHead = DBhelper.getDatatableFromSql(u8login.UfDbName, "select * from ap_vouch where clink='" + cLink + "'");
            DataTable dtBody = DBhelper.getDatatableFromSql(u8login.UfDbName, "select * from ap_vouchs where clink='" + cLink + "'");
            Decimal sumTax = 0m;
            DataRow drHead = dtHead.Rows[0];           

            foreach (DataRow drBody in dtBody.Rows)
            { 
                //sumTax += Convert.ToDecimal(drBody["iTax"]); 
                //20220915 含税金额
                sumTax += Convert.ToDecimal(drBody["iAmount"]); 
            }
            #region//借方
            if (sumTax != 0)
            {                
                //含税金额
                //ccode = "112201";                
                
                strSql = "insert into " + obj.strTempTable
                + "(ioutperiod,ccus_id,coutbillsign,coutid,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,md,cdept_id,citem_id,citem_class,cperson_id,cname)  values("
                + Month + ",'" + drHead["cDwCode"].ToString() + "','R0','R0" + vouchID + "','记','记','" + cDigest + "','ARR0" + vouchID + "','AR','" + cmaker + "'," + iRow.ToString() + ",'" + drHead["cCode"].ToString() + "',null,'" + ddate.ToShortDateString() + "',null,1,1,1,"
                    //+ drHead["iAmount"].ToString()
                + sumTax.ToString()
                + ",'" + drHead["cDeptCode"].ToString() + "','" + drHead["cItemCode"].ToString() + "','" + drHead["cItem_Class"].ToString() + "','" + drHead["cPerson"].ToString() + "','"+drHead["cDefine13"].ToString() + "')";
                LogHelper.WriteLog(typeof(ReceivableEntity), "glaccvouch:" + strSql);
                conn.Execute(strSql, out objOut);
                iRow++;
            }
            #endregion

            #region//贷方
            
            
            foreach (DataRow drBody in dtBody.Rows)
            {
                //20220915 无税金额
                if (Convert.ToDecimal(drBody["iNoTaxAmount"]) != 0)
                {
                    CodeResult crXssrkm = ARAPCodeEntity.getXssrkm(payable.ztcode);
                    if (crXssrkm != null)
                    {
                        if (crXssrkm.recode != "")
                        {
                            ccode = crXssrkm.recode;
                        }
                    }
                    strSql = "insert into " + obj.strTempTable
                    + "(ioutperiod,ccus_id,coutbillsign,coutid,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,mc,cdept_id,citem_id,citem_class,cperson_id,cname)  values("
                    + Month + ",'" + drHead["cDwCode"].ToString() + "','R0','R0" + vouchID + "','记','记','" + cDigest + "','ARR0" + vouchID + "','AR','" + cmaker + "'," + iRow.ToString() + ",'" + ccode + "',null,'" + ddate.ToShortDateString() + "',null,1,1,1," + drBody["iNoTaxAmount"].ToString()
                    + ",'" + drBody["cDeptCode"].ToString() + "','" + drBody["cItemCode"].ToString() + "','" + drBody["cItem_Class"].ToString() + "','" + drBody["cPerson"].ToString() + "','" + drBody["cDefine28"].ToString() + "')";
                    LogHelper.WriteLog(typeof(ReceivableEntity), "glaccvouch:" + strSql);
                    //sumTax += Convert.ToDecimal(drBody["iTax"]);
                    conn.Execute(strSql, out objOut);
                    iRow++;

                }
                //税额
                if (Convert.ToDecimal(drBody["iTax"]) != 0)
                {
                    CodeResult crXssjkm = ARAPCodeEntity.getXssjkm(payable.ztcode,drBody["cDefine30"].ToString());
                    if (crXssjkm!=null)
                    {
                        if (crXssjkm.recode!="")
                        {
                            ccode = crXssjkm.recode;
                        }
                    }

                    strSql = "insert into " + obj.strTempTable
                    + "(ioutperiod,ccus_id,coutbillsign,coutid,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,mc,cdept_id,citem_id,citem_class,cperson_id,cname)  values("
                    + Month + ",'" + drHead["cDwCode"].ToString() + "','R0','R0" + vouchID + "','记','记','" + cDigest + "','ARR0" + vouchID + "','AR','" + cmaker + "'," + iRow.ToString() + ",'" + ccode + "',null,'" + ddate.ToShortDateString() + "',null,1,1,1," + drBody["iTax"].ToString()
                    + ",'" + drBody["cDeptCode"].ToString() + "','" + drBody["cItemCode"].ToString() + "','" + drBody["cItem_Class"].ToString() + "','" + drBody["cPerson"].ToString() + "','" + drBody["cDefine28"].ToString() + "')";
                    LogHelper.WriteLog(typeof(ReceivableEntity), "glaccvouch:" + strSql);
                    //sumTax += Convert.ToDecimal(drBody["iTax"]);
                    conn.Execute(strSql, out objOut);
                    iRow++;
                }

            }
            #endregion

            if (sumTax != 0)
            {
                obj.set_Connection(conn);
                obj.LoginByUserToken(u8login.userToken);
                bTran = obj.SaveVoucher();
                if (bTran)
                {
                    String pzID = DBhelper.getDataFromSql(u8login.UfDbName, "select ino_id result from gl_accvouch where coutbillsign='R0' and coutid='R0" + vouchID + "'");
                    String cpzID = DBhelper.getDataFromSql(u8login.UfDbName, "select coutno_id result from gl_accvouch where coutbillsign='R0' and coutid='R0" + vouchID + "'");
                    String cPzNum = "记-" + string.Format("{0:D4}", Convert.ToInt32(pzID));
                    strSql = "update ap_vouch set cPzId='" + cpzID + "',cPzNum='" + cPzNum + "',doutbilldate='" + ddate.ToShortDateString() + "' where cLink='" + cLink + "'";
                    LogHelper.WriteLog(typeof(ReceivableEntity), "apvouch:" + strSql);
                    DBhelper.setDataFromSql(u8login.UfDbName, strSql);

                    strSql = " update Ar_Detail set isignseq=1,cglsign='记',iglno_id=" + pzID + ",ino_id=" + pzID + ",cDigest='" + cDigest + "',cPZid='" + cpzID + "' where cVouchID='" + vouchID + "' and cVouchType='R0' and cCoVouchID='" + vouchID + "' and cCoVouchType='R0'";
                    LogHelper.WriteLog(typeof(ReceivableEntity), "apvouch:" + strSql);
                    DBhelper.setDataFromSql(u8login.UfDbName, strSql);
                }
                else
                {
                    LogHelper.WriteLog(typeof(ReceivableEntity), "arvouch:" + obj.strErrMessage.ToString());
                }
            }
            return bTran;
        }
    }
}