using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using QTU8interface.Models.Expense;
using QTU8interface.Models.Result;
using QTU8interface.UFIDA;
namespace QTU8interface.Entities
{
    public class ExpenseVouchEntity
    {
        public static void Add_Voucher(ClsExpense expense,ref ClsResult re)
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
            string depcode="";
            string itemclass = "";
            string itemcode="";
            decimal amount = 0m;
            decimal sumamount = 0m;
            decimal sumtax = 0m;
            decimal tax = 0m;
            decimal money = 0m;
            int vouchRows = 0;
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNo = null;
            try
            {
                
                xmlDoc.Load(HttpContext.Current.Server.MapPath("..") + "\\UFIDA\\voucherconfig.xml");
                LogHelper.WriteLog(typeof(ExpenseVouchEntity), "start login");
                U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity(expense.ztcode.ToString());
                LogHelper.WriteLog(typeof(ExpenseVouchEntity), "finish login");
                if (u8login==null)
                {
                    re.recode = "111";
                    re.remsg = re.ztcode + "对应帐套登录失败";
                    return;
                }
                //检查凭证是否重复
                strSql="select i_id from gl_accvouch where cdigest like '%" + expense.head.oacode + "%' and isnull(iflag,0)=0";
                LogHelper.WriteLog(typeof(ExpenseVouchEntity), strSql);
                strResult = DBhelper.getDataFromSql(u8login.UfDbName, strSql);
                if (strResult != "")
                {
                    strResult = expense.head.oacode + "已生成过凭证";
                    re.recode = "333";
                    re.remsg = strResult;
                    return;
                }
                //检查人员是否存在
                //strSql = "select cDept_num from hr_hi_person where cPsn_Name='" + expense.body[0].person + "'";
                //LogHelper.WriteLog(typeof(ExpenseVouchEntity), strSql);
                //depcode = DBhelper.getDataFromSql(u8login.UfDbName, strSql);
                //if (depcode == "")
                //{
                //    strResult = expense.body[0].person + "在U8人员档案中不存在";
                //    re.recode = "222";
                //    re.remsg = strResult;
                //    return;
                //}
                //检查项目是否存在
                /*
                if (!string.IsNullOrEmpty(expense.head.projname))
                {
                    strSql ="select citemcode from fitemss97 where citemname='" + expense.head.projname + "'";
                    //LogHelper.WriteLog(typeof(ExpenseVouchEntity), strSql);
                    itemcode = DBhelper.getDataFromSql(u8login.UfDbName, strSql);
                    if (itemcode == "")
                    {
                        strResult = expense.head.projname + "在U8项目管理档案中不存在";
                        re.recode = "222";
                        re.remsg = strResult;
                        return;
                    }
                }*/
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
                        + "bvouchedit bit default 0, bvouchaddordele bit default 0, bvouchmoneyhold bit default 0, bvalueedit bit default 0, bcodeedit bit default 0, ccodecontrol nvarchar(50), bPCSedit bit default 0, bDeptedit bit default 0, bItemedit bit default 0, bCusSupInput bit default 0, "
                        + "coutaccset nvarchar(23), ioutyear smallint, coutsysname nvarchar(50) NOT NULL, coutsysver nvarchar(50), ioutperiod tinyint NOT NULL, coutsign nvarchar(80) NOT NULL, coutno_id nvarchar(100) NOT NULL, doutdate DATETIME, coutbillsign nvarchar(80), coutid nvarchar(50), iflag tinyint"
                        + ",iBG_ControlResult smallint null,daudit_date DateTime NULL,cblueoutno_id nvarchar(50) NULL,bWH_BgFlag bit,cDefine1 nvarchar(40),"
                        + "cDefine2 nvarchar(40),cDefine3 nvarchar(40),cDefine4 DateTime,cDefine5 int,cDefine6 DateTime,cDefine7 Float,cDefine8 nvarchar(4),cDefine9 nvarchar(8),"
                        + "cDefine10 nvarchar(60),cDefine11 nvarchar(120),cDefine12 nvarchar(120),cDefine13 nvarchar(120),cDefine14 nvarchar(120),cDefine15 int,cDefine16 float)";
                conn.Execute(strSql, out objOut);


                #region//借方
                foreach (Expense_Body body in expense.body)
                {
                    amount = body.amount;
                    sumamount += amount;
                    sumtax += body.tax;
                    tax = body.tax;
                    money = amount - tax;    
                    if (!string.IsNullOrEmpty(body.person))
                    {
                        //检查人员是否存在
                        strSql = "select cDept_num from hr_hi_person where cPsn_Name='" + body.person + "'";
                        //LogHelper.WriteLog(typeof(ExpenseVouchEntity), strSql);
                        depcode = DBhelper.getDataFromSql(u8login.UfDbName, strSql);
                        if (depcode == "")
                        {
                            strResult = body.person + "在U8人员档案中不存在";
                            re.recode = "222";
                            re.remsg = strResult;
                            return;
                        }
                    }

                    #region//费用
                    CodeResult coderesult = ExpenseCodeEntity.getExpenseCode(expense.ztcode, body.person, body.bgcode, expense.head.prodname, expense.head.projname, u8login,ref itemclass,ref itemcode);
                    xmlNo = xmlDoc.SelectSingleNode("ufinterface/voucher[@type='费用报销']/entry[ccode='[expensecode]']");
                    cdigest = getDigest(xmlNo.SelectSingleNode("cdigest").InnerText, expense.head, body);

                    if (!string.IsNullOrEmpty(coderesult.recode))
                    {
                        ccode = coderesult.recode;                        
                        if (money != 0)
                        {
                            vouchRows ++;
                            strSql = "insert into " + obj.strTempTable
                                            + "(ioutperiod,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,md,cdept_id,citem_id,citem_class)  " +
                                            " values(" + Month + ",N'记',N'记', '"
                                            + cdigest
                                            + "',N'" + strGuid + "',N'',N'" + u8login.cUserName + "'," + rowno.ToString() + ","
                                            + ccode
                                            + ",N'','"
                                            + Date + "','" + Date + "',1,1,1,"
                                            + money.ToString() + ",'" + depcode + "','" + itemcode + "','"+itemclass+"')";
                            conn.Execute(strSql, out objOut);
                            rowno++;
                        }
                    }
                    else
                    {
                        re.recode = "222";
                        re.remsg = coderesult.remsg;
                        return;
                    }
                    #endregion
                    /*
                     * 2021-07-06 职工福利费  职工教育经费
                     */
                    
                    #region//职工福利费  职工教育经费
                    CodeResult codeExchageResult = ExpenseCodeEntity.getExchangeWageCode(expense.ztcode, coderesult.recode);
                    if (!string.IsNullOrEmpty(codeExchageResult.recode))
                    {
                        ccode = codeExchageResult.recode;
                        //贷方
                        if (money != 0)
                        {
                            strSql = "insert into " + obj.strTempTable
                                            + "(ioutperiod,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,mc,cdept_id,citem_id,citem_class)  " +
                                            " values(" + Month + ",N'记',N'记', '"
                                            + cdigest
                                            + "',N'" + strGuid + "',N'',N'" + u8login.cUserName + "'," + rowno.ToString() + ","
                                            + ccode
                                            + ",N'','"
                                            + Date + "','" + Date + "',1,1,1,"
                                            + money.ToString() + ",'" + depcode + "','" + itemcode + "','97')";
                            conn.Execute(strSql, out objOut);
                            rowno++;
                            vouchRows++;
                        }
                        //借方
                        if (money != 0)
                        {
                            strSql = "insert into " + obj.strTempTable
                                            + "(ioutperiod,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,md,cdept_id,citem_id,citem_class)  " +
                                            " values(" + Month + ",N'记',N'记', '"
                                            + cdigest
                                            + "',N'" + strGuid + "',N'',N'" + u8login.cUserName + "'," + rowno.ToString() + ","
                                            + ccode
                                            + ",N'','"
                                            + Date + "','" + Date + "',1,1,1,"
                                            + money.ToString() + ",'" + depcode + "','" + itemcode + "','97')";
                            conn.Execute(strSql, out objOut);
                            rowno++;
                            vouchRows++;
                        }
                    }

                    #endregion


                    #region//税金
                    coderesult = ExpenseCodeEntity.getTaxCode(expense.ztcode);
                    xmlNo = xmlDoc.SelectSingleNode("ufinterface/voucher[@type='费用报销']/entry[ccode='[taxcode]']");
                    if (!string.IsNullOrEmpty(coderesult.recode))
                    {
                        ccode = coderesult.recode;
                        if (tax != 0)
                        {
                            strSql = "insert into " + obj.strTempTable
                                            + "(ioutperiod,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,md,cdept_id,citem_id,citem_class)  " +
                                            " values(" + Month + ",N'记',N'记', '"
                                            + cdigest
                                            + "',N'" + strGuid + "',N'',N'" + u8login.cUserName + "'," + rowno.ToString() + ","
                                            + ccode
                                            + ",N'','"
                                            + Date + "','" + Date + "',1,1,1,"
                                            + tax.ToString() + ",'" + depcode + "','" + itemcode + "','" + itemclass + "')";
                            conn.Execute(strSql, out objOut);
                            rowno++;
                            vouchRows++;
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

                #endregion
                #region//贷方
                xmlNo = xmlDoc.SelectSingleNode("ufinterface/voucher[@type='费用报销']/entry[ccode='[accountcode]']");
                     //foreach (Expense_Settle body in expense.settle)
                     //{
                         amount =sumamount;
                         cdigest = getDigest(xmlNo.SelectSingleNode("cdigest").InnerText, expense.head, expense.body[0]);
                         CodeResult coderesult1 = ExpenseCodeEntity.getAccountCode(expense.ztcode, expense.head.accountcode);
                         if (!string.IsNullOrEmpty(coderesult1.recode))
                         {
                             ccode = coderesult1.recode;
                             if (amount != 0)
                             {
                                 strSql = "insert into " + obj.strTempTable
                                                 + "(ioutperiod,coutsign ,cSign,cdigest,coutno_id,coutsysname,cbill,inid,ccode,cexch_name ,doutbilldate,dt_date,bvouchedit,bvalueedit,bcodeedit,mc,cdept_id,citem_id,citem_class)  " +
                                                 " values(" + Month + ",N'记',N'记', '"
                                                 + cdigest
                                                 + "',N'" + strGuid + "',N'',N'" + u8login.cUserName + "'," + rowno.ToString() + ","
                                                 + ccode
                                                 + ",N'','"
                                                 + Date + "','" + Date + "',1,1,1,"
                                                 + amount.ToString() + ",'" + depcode + "','" + itemcode + "','" + itemclass + "')";
                                 conn.Execute(strSql, out objOut);
                                 rowno++;
                                 vouchRows++;
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
                //2021-10-27 判断金额和税额都为零，返回成功
                if ((sumamount==0)&&(sumtax==0)&&(vouchRows==0))
                {
                    strResult = "金额和税额都为零!";
                    re.recode = "0";
                    re.remsg = strResult;
                    re.u8code = "";
                    return;
                }

                 obj.set_Connection(conn);
                 obj.LoginByUserToken(u8login.userToken);
                 if (obj.SaveVoucher())
                 {
                     strResult = "vouch save success!";
                     re.recode = "0";
                     re.remsg = "";
                     re.u8code = DBhelper.getDataFromSql(u8login.UfDbName, "select convert(nvarchar(10),iyear)+'年'+convert(nvarchar(10),iperiod)+'月'+csign+convert(nvarchar(10),ino_id) result from gl_accvouch where cdigest like '%"+expense.head.oacode+"%' and isnull(iflag,0)=0");
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
            catch(Exception ex)
            {
                re.recode = "999";
                re.remsg = ex.Message;
                LogHelper.WriteLog(typeof(ExpenseVouchEntity), ex);
                return;
            }
            finally
            {
                if (conn.State == 1)
                { conn.Close(); }
            }
        }

        private static string getItemValue(string itemName,Expense_Head head,Expense_Body body)//得到参数值
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
                    Object v=null;
                    if ( head.GetType().GetProperty(itemName)!=null)
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
        private static string getDigest(string digest, Expense_Head head, Expense_Body body)//摘要
        {
            string result = "";
            string name = "";
            string[] array = digest.Split('+');
            foreach (string str in array)
            {
                result += getItemValue(str, head,body);
            }
            return result;
        }

    }
}