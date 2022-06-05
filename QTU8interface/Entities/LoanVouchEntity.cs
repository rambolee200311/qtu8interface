using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using QTU8interface.Models.Loan;
using QTU8interface.Models.Result;
using QTU8interface.Models.CashFlow;
using QTU8interface.UFIDA;

namespace QTU8interface.Entities
{
    public class LoanVouchEntity
    {
        public static void Add_Voucher(ClsLoan expense, ref ClsResult re,string ctype)
        {
            string ccode = "";
            int rowno = 1;
            ADODB.Connection conn = new ADODB.Connection();
            string Year = "";//年
            string Month = "";//月
            string Date = "";//日
            object objOut = null;
            string strResult = "";
            string strSql = "";
            string strGuid = Guid.NewGuid().ToString("N");
            string cdigest = "";
            string depcode = "";
            string personcode = "";
            string itemcode = "";
            decimal amount = 0m;
            decimal sumamount = 0m;
            decimal tax = 0m;
            decimal money = 0m;
            String dc="";
            string cmaker = "";
            XmlDocument xmlDoc = new XmlDocument();
            XmlDocument xmlDocCashFlow = new XmlDocument();
            xmlDocCashFlow.Load(HttpContext.Current.Server.MapPath("..") + "\\UFIDA\\CashFlowColXml.xml");
            XmlNode xmlNo = null;
            List<CashFlow> cashFlows = new List<CashFlow>();
            try
            {
                xmlDoc.Load(HttpContext.Current.Server.MapPath("..") + "\\UFIDA\\voucherconfig.xml");
                U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity(expense.ztcode.ToString(), "", "");
                if (u8login == null)
                {
                    re.recode = "111";
                    re.remsg = re.ztcode + "对应帐套登录失败";
                    return;
                }
                //20220605 制单人等于推单人
                if (string.IsNullOrEmpty(expense.head.cmaker))
                {
                    cmaker = u8login.cUserName;
                }
                else
                {
                    cmaker = expense.head.cmaker;
                }
                //检查凭证是否重复
                strResult = DBhelper.getDataFromSql(u8login.UfDbName, "select i_id from gl_accvouch where cdigest like '%" + expense.head.oacode + "%' and isnull(iflag,0)=0");
                if (strResult != "")
                {
                    strResult = expense.head.oacode + "已生成过凭证";
                    re.recode = "333";
                    re.remsg = strResult;
                    return;
                }
                //检查人员是否存在
                depcode = DBhelper.getDataFromSql(u8login.UfDbName, "select cDept_num from hr_hi_person where cPsn_Name='" + expense.head.person + "'");
                personcode = DBhelper.getDataFromSql(u8login.UfDbName, "select  cPsn_Num from hr_hi_person where cPsn_Name='" + expense.head.person + "'");
                if (depcode == "")
                {
                    strResult = expense.head.person + "在U8人员档案中不存在";
                    re.recode = "222";
                    re.remsg = strResult;
                    return;
                }
                //2 创建组件
                CVoucher.CVInterface obj = new CVoucher.CVInterface();
                conn.Open(u8login.UfDbName);
                //3 创建临时表


                Date = Convert.ToDateTime(expense.head.ddate).ToShortDateString();
                Month = Convert.ToDateTime(expense.head.ddate).Month.ToString();
                Year = Convert.ToDateTime(expense.head.ddate).Year.ToString();

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



                #region//借方
                foreach (Loan_Body body in expense.body)
                {
                    amount = body.amount;
                    sumamount += amount;
                    tax = 0;
                    money = amount - tax;
                    //借款
                    CodeResult coderesult = ExpenseCodeEntity.getOtherARCode(expense.ztcode);
                    switch (ctype.ToLower())
                    {
                        case "loan":
                            xmlNo = xmlDoc.SelectSingleNode("ufinterface/voucher[@type='借款']/entry[ccode='[otherarcode]']");
                            break;
                        case "repayloan":
                            xmlNo = xmlDoc.SelectSingleNode("ufinterface/voucher[@type='还款']/entry[ccode='[otherarcode]']");
                            break;
                    }
                    
                    cdigest = getDigest(xmlNo.SelectSingleNode("cdigest").InnerText, expense.head, body);
                    
                    if (!string.IsNullOrEmpty(coderesult.recode))
                    {
                        ccode = coderesult.recode;
                        if (money != 0)
                        {
                            switch(ctype)
                            { 
                                case "loan":
                                strSql = "insert into " + obj.strTempTable
                                                + "(ioutperiod,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,md,cdept_id,citem_id,citem_class,cperson_id)  ";
                                    break;
                                case "repayloan":
                                    strSql = "insert into " + obj.strTempTable
                                                    + "(ioutperiod,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,mc,cdept_id,citem_id,citem_class,cperson_id)  ";
                                    break;
                             }
                            strSql+=                " values(" + Month + ",N'记',N'记', '"
                                            + cdigest
                                            + "',N'" + strGuid + "',N'',N'" + cmaker + "'," + rowno.ToString() + ","
                                            + ccode
                                            + ",N'','"
                                            + Date + "','" + Date + "',1,1,1,"
                                            + money.ToString() + ",'" + depcode + "','" + itemcode + "','97','" + personcode + "')";
                            conn.Execute(strSql, out objOut);

                            //现金流量
                            CashFlow cashFlow = new CashFlow();
                            cashFlow.Amount_f = money;
                            cashFlow.Amount = money;
                            cashFlow.cCashItem = coderesult.cashitemcode;
                            cashFlows.Add(cashFlow);


                            rowno++;
                        }
                    }
                    else
                    {
                        re.recode = "222";
                        re.remsg = coderesult.remsg;
                        return;
                    }

                }
                #endregion


                #region//贷方
                switch (ctype.ToLower())
                {
                    case "loan":
                        xmlNo = xmlDoc.SelectSingleNode("ufinterface/voucher[@type='借款']/entry[ccode='[accountcode]']");
                        break;
                    case "repayloan":
                        xmlNo = xmlDoc.SelectSingleNode("ufinterface/voucher[@type='还款']/entry[ccode='[accountcode]']");
                        break;
                }
               
                //foreach (Loan_Settle body in expense.settle)
                //{
                    amount = sumamount;
                    cdigest = getDigest(xmlNo.SelectSingleNode("cdigest").InnerText, expense.head, expense.body[0]);
                    CodeResult coderesult1 = ExpenseCodeEntity.getAccountCode(expense.ztcode, expense.head.accountcode);
                    if (!string.IsNullOrEmpty(coderesult1.recode))
                    {
                        ccode = coderesult1.recode;
                        if (amount != 0)
                        {
                            switch (ctype.ToLower())
                            {
                                case "loan":
                                    dc="credit";
                                    strSql = "insert into " + obj.strTempTable
                                                    + "(ioutperiod,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,mc,cdept_id,citem_id,citem_class,cperson_id)  ";
                                    break;
                                case "repayloan":
                                    dc="debit";
                                    strSql = "insert into " + obj.strTempTable
                                                    + "(ioutperiod,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,md,cdept_id,citem_id,citem_class,cperson_id)  ";
                                    break;
                            }
                                            strSql+=" values(" + Month + ",N'记',N'记', '"
                                            + cdigest
                                            + "',N'" + strGuid + "',N'',N'" + cmaker + "'," + rowno.ToString() + ","
                                            + ccode
                                            + ",N'','"
                                            + Date + "','" + Date + "',1,1,1,"
                                            + amount.ToString() + ",'" + depcode + "','" + itemcode + "','97','"+personcode+"')";
                            conn.Execute(strSql, out objOut);

                            //现金流量
                            setCashXml(u8login.UfDbName, obj, xmlDocCashFlow,
                                                  strGuid, Month, Year, Date,
                                                  cdigest, "", "",
                                                  "", "", "", "",
                                                  ccode, cashFlows, rowno.ToString(), dc);
                            rowno++;
                        }
                    }
                    else
                    {
                        re.recode = "222";
                        re.remsg = coderesult1.remsg;
                        return;
                    }
                //}
                #endregion
                //re.recode = "0";
                //re.remsg = "";
                //6 调用保存
                //if (xmlDocCashFlow.SelectSingleNode("root/rows").ChildNodes.Count > 1)
                //{
                //    xmlDocCashFlow.SelectSingleNode("root/rows").RemoveChild(xmlDocCashFlow.SelectSingleNode("root/rows").FirstChild);
                //    obj.CashFlowColXml = xmlDocCashFlow.OuterXml;
                //}


                obj.set_Connection(conn);
                obj.LoginByUserToken(u8login.userToken);
                obj.CashFlowColXml = xmlDocCashFlow.OuterXml;
                if (obj.SaveVoucher())
                {
                    strResult = "vouch save success!";
                    re.recode = "0";
                    re.remsg = "";
                    re.u8code = DBhelper.getDataFromSql(u8login.UfDbName, "select convert(nvarchar(10),iyear)+'年'+convert(nvarchar(10),iperiod)+'月'+csign+convert(nvarchar(10),ino_id) result from gl_accvouch where cdigest like '%" + expense.head.oacode + "%' and isnull(iflag,0)=0");
                    return;
                }
                else
                {
                    strResult = obj.strErrMessage.ToString();
                    re.recode = "7777";
                    re.remsg = strResult;
                    return;
                }

            }
            catch (Exception ex)
            {
                re.recode = "999";
                re.remsg = ex.Message;
                LogHelper.WriteLog(typeof(LoanVouchEntity), ex);
                return;
            }
            finally
            {
                if (conn.State == 1)
                { conn.Close(); }
            }
        }

        private static string getItemValue(string itemName, Loan_Head head, Loan_Body body)//得到参数值
        {
            string result = "";
            //if ((itemName.Substring(0,1)=="{")||(itemName.Substring(0,1)=="["))
            switch (itemName.Substring(0, 1))
            {
                case "{":
                    result = itemName.Substring(1, itemName.Length - 2);
                    break;
                case "[":
                    itemName = itemName.Substring(1, itemName.Length - 2);
                    Object v = null;
                    if (head.GetType().GetProperty(itemName) != null)
                    {
                        v = head.GetType().GetProperty(itemName).GetValue(head, null);
                    }
                    else if (body.GetType().GetProperty(itemName) != null)
                    {
                        v = body.GetType().GetProperty(itemName).GetValue(body, null);
                    }
                    //else if (settle.GetType().GetProperty(itemName) != null)
                    //{
                    //    v = settle.GetType().GetProperty(itemName).GetValue(settle, null);
                    //}

                    //Object v = head.GetType().GetProperty(itemName).GetValue(head, null);
                    if (v != null)
                    {
                        if (itemName != "ddate")
                        { result += v.ToString(); }
                        else
                        { result += Convert.ToDateTime(v).ToShortDateString(); }
                    }
                    //else
                    //{
                    //    v = body.GetType().GetProperty(itemName).GetValue(body, null);
                    //    if (v != null)
                    //    {
                    //        result += v.ToString();
                    //    }
                    //    else
                    //    {
                    //        v = settle.GetType().GetProperty(itemName).GetValue(settle, null);
                    //        if (v != null)
                    //        {
                    //            result += v.ToString();
                    //        }
                    //    }
                    //}
                    break;
            }
            return result;
        }
        private static string getDigest(string digest, Loan_Head head, Loan_Body body)//摘要
        {
            string result = "";
            string name = "";
            string[] array = digest.Split('+');
            foreach (string str in array)
            {
                result += getItemValue(str, head, body);
            }
            return result;
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