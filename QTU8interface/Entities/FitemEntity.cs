using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Data;
using QTU8interface.Models.Fitem;
using QTU8interface.Models.Result;
using QTU8interface.UFIDA;

namespace QTU8interface.Entities
{
    public class FitemEntity
    {
        public static void Add_Archive(Fitem fitem, ref ClsResult re)
        {
            XmlDocument xmlDoc = new XmlDocument();
            bool bResult = false;
            string strResult = "";
            string classcode = "";
            string code = "";
            try
            {
                //xmlDoc.Load(HttpContext.Current.Server.MapPath("..") + "\\UFIDA\\customer.xml");
               
                U8Login.clsLogin u8login = LoginHelper.getU8LoginEntity(fitem.ztcode.ToString());
                code = fitem.oacode;
                if (u8login == null)
                {
                    re.recode = "111";
                    re.remsg = re.ztcode + "对应帐套登录失败";
                    return;
                }

                xmlDoc.LoadXml(getFitemDataxml(u8login, "add"));

                if (DBhelper.getDataFromSql(u8login.UfDbName, "select 1 from fitemss97 where citemname='" + fitem.oaname + "'") != "")
                {
                    //re.remsg = fitem.oaname + "已存在同名项目";
                    //re.recode = "222";
                    //return;
                    bResult = true;
                    strResult = "同步成功";
                    re.u8code = code;
                    re.recode = "0";
                    re.remsg = "";
                    return;
                }
                classcode = DBhelper.getDataFromSql(u8login.UfDbName, "select citemccode from fitemss97class where bitemCEnd=1 and citemcname='" + fitem.classcode + "'");
                if (classcode == "")
                {
                    re.remsg = fitem.classcode + "不存在项目分类或非末级";
                    re.recode = "222";
                    return;
                }

                U8PzInsert.IFitemClass ust = new U8PzInsert.IFitemClass();             

               

                DataTable dtItemss = DBhelper.getDatatableFromSql(u8login.UfDbName, "select * from fitemss97 where 1=0");

                XmlElement xmlItemss = xmlDoc.CreateElement("item");
                foreach (DataColumn dc in dtItemss.Columns)
                {
                    XmlAttribute att = xmlDoc.CreateAttribute(dc.ColumnName);
                    switch (dc.ColumnName)
                    {
                        case "i_id":
                            string i_id = DBhelper.getDataFromSql(u8login.UfDbName, "select max(isnull(i_id,0))+1 iid from fitemss97" );
                            if (string.IsNullOrEmpty(i_id))
                            {
                                i_id = "1";
                            }
                            att.Value = i_id;
                            break;
                        case "citemcode":
                            att.Value = code;
                            break;
                        case "citemname":
                            att.Value =fitem.oaname;
                            break;
                        case "citemccode":
                            att.Value = classcode;
                            break;
                        case "bclose":
                            att.Value = "False";
                            break;
                        default:
                            att.Value = "";
                            break;
                    }
                    xmlItemss.Attributes.Append(att);
                }
                xmlDoc.SelectSingleNode("ufinterface//fitem[@citem_class='97']").SelectSingleNode("//itemclass[@citemccode='" + classcode + "']").AppendChild(xmlItemss);

               
                strResult = ust.Transact(xmlDoc.OuterXml,u8login);
                XmlDocument xmlResult = new XmlDocument();
                xmlResult.LoadXml(strResult);
                if (xmlResult.SelectSingleNode("ufinterface//item").Attributes["succeed"]!=null)
                {
                    if (xmlResult.SelectSingleNode("ufinterface//item").Attributes["succeed"].Value=="0")
                    {
                        bResult=true;
                        strResult="同步成功";
                        re.u8code = code;
                        re.recode="0";
                        re.remsg="";
                        return ;
                    }
                    else
                    {
                        bResult = false;
                        strResult = xmlResult.SelectSingleNode("ufinterface//item").Attributes["dsc"].Value;
                        re.recode="333";
                        re.remsg = strResult;
                        return ;
                    }
                }
                else
                {
                    bResult = false;
                    strResult = "未知原因";
                    re.recode="333";
                    re.remsg = strResult;
                    return ;
                }
                    

               
            }
            catch (Exception ex)
            {
                re.recode = "999";
                re.remsg = ex.Message;
                LogHelper.WriteLog(typeof(CustomerEntity), ex);
                return;
            }
        }
        private static string getFitemDataxml(U8Login.clsLogin m_ologin, string strproc)//,string EaiCode)//datatable转xml
        {
            string strResult = "";

            XmlDocument xmlDoc = new XmlDocument();
            #region//declare
            XmlDeclaration xmldecl;
            xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(xmldecl);
            XmlElement xmldata = xmlDoc.CreateElement("ufinterface");

            //billtype="" docid="" receiver="" sender="" proc="" codeexchanged="" exportneedexch="" version="2.0"
            XmlAttribute roottag = xmlDoc.CreateAttribute("roottag");
            roottag.Value = "fitem";
            xmldata.Attributes.Append(roottag);
            XmlAttribute proc = xmlDoc.CreateAttribute("proc");
            proc.Value = strproc;
            xmldata.Attributes.Append(proc);
            XmlAttribute billtype = xmlDoc.CreateAttribute("billtype");
            billtype.Value = "";
            xmldata.Attributes.Append(billtype);
            XmlAttribute docid = xmlDoc.CreateAttribute("docid");
            docid.Value = "";
            xmldata.Attributes.Append(docid);
            XmlAttribute receiver = xmlDoc.CreateAttribute("receiver");
            receiver.Value = "";
            xmldata.Attributes.Append(receiver);
            XmlAttribute sender = xmlDoc.CreateAttribute("sender");
            sender.Value = "";
            xmldata.Attributes.Append(sender);
            XmlAttribute codeexchanged = xmlDoc.CreateAttribute("codeexchanged");
            codeexchanged.Value = "N";
            xmldata.Attributes.Append(codeexchanged);
            XmlAttribute exportneedexch = xmlDoc.CreateAttribute("exportneedexch");
            exportneedexch.Value = "N";
            xmldata.Attributes.Append(exportneedexch);
            #endregion

            DataTable dtFtiem =DBhelper.getDatatableFromSql(m_ologin.UfDbName, "select citem_class,citem_name,citem_text,crule from fitem");
            foreach (DataRow dr in dtFtiem.Rows)
            {
                //citem_class="98" citem_name="现金流量项目" citem_text="项目名称" crule="22"
                XmlElement xmlFitem = xmlDoc.CreateElement("fitem");
                XmlElement xmlStructure = xmlDoc.CreateElement("structure");

                foreach (DataColumn dc in dtFtiem.Columns)
                {
                    XmlAttribute att = xmlDoc.CreateAttribute(dc.ColumnName);
                    att.Value = dr[dc.ColumnName].ToString();
                    xmlFitem.Attributes.Append(att);
                }
                #region//structure
                //<structure>
                //<field citem_sqr="1" cfield_name="I_id" ctext="自动编号" imode="0" itype="1" ilength="4" iscale="0" blist="False" bsum="False" bref="False" isubitem="0" idefine="" bprimarykey="False" isource="" ctablename="" cfieldname=""/>
                DataTable dtStructure = DBhelper.getDatatableFromSql(m_ologin.UfDbName, "select citem_sqr,cfield_name,ctext,imode,itype, ilength,iscale, blist, bsum, bref,isubitem,idefine,bprimarykey,isource,ctablename, cfieldname from fitemstructure where citem_class='" + dr["citem_class"].ToString() + "'");
                foreach (DataRow drStructure in dtStructure.Rows)
                {
                    XmlElement xmlField = xmlDoc.CreateElement("field");
                    foreach (DataColumn dc in dtStructure.Columns)
                    {
                        XmlAttribute att = xmlDoc.CreateAttribute(dc.ColumnName);
                        att.Value = drStructure[dc.ColumnName].ToString();
                        xmlField.Attributes.Append(att);
                    }

                    xmlStructure.AppendChild(xmlField);
                }
                xmlFitem.AppendChild(xmlStructure);
                #endregion

                #region//itemclass
                //<itemclass citemccode="01" citemcname="经营活动" iitemcgrade="1" bitemcend="False">
                DataTable dtItemClass = DBhelper.getDatatableFromSql(m_ologin.UfDbName, "select citemccode,citemcname,iitemcgrade,bitemcend from fitemss" + dr["citem_class"].ToString() + "class order by iitemcgrade");
                foreach (DataRow drItemClass in dtItemClass.Rows)
                {
                    XmlElement xmlItemClass = xmlDoc.CreateElement("itemclass");
                    foreach (DataColumn dc in dtItemClass.Columns)
                    {
                        XmlAttribute att = xmlDoc.CreateAttribute(dc.ColumnName);
                        att.Value = drItemClass[dc.ColumnName].ToString();
                        xmlItemClass.Attributes.Append(att);
                    }
                    if (drItemClass["iitemcgrade"].ToString() == "1")
                    {
                        xmlFitem.AppendChild(xmlItemClass);
                    }
                    else
                    {
                        string upCode = "";
                        //DataRow[] drs = dtItemClass.Select("iitemcgrade=" + drItemClass["iitemcgrade"].ToString() + "-1 and CHARINDEX(citemccode,'" + drItemClass["citemccode"].ToString() + "')>0");
                        upCode = DBhelper.getDataFromSql(m_ologin.UfDbName, "select citemccode from fitemss" + dr["citem_class"].ToString() + "class where iitemcgrade=" + drItemClass["iitemcgrade"].ToString() + "-1 and CHARINDEX(citemccode,'" + drItemClass["citemccode"].ToString() + "')=1");
                        if (upCode != "")
                        {

                            xmlFitem.SelectSingleNode("//itemclass[@citemccode='" + upCode + "']").AppendChild(xmlItemClass);

                        }
                    }
                }

                #endregion

                #region //fitemss
                //<item i_id="1" citemcode="01" citemname="销售商品、提供劳务收到的现金" bclose="False" citemccode="0101" iotherused="" cdirection="流入"/>
                DataTable dtItemss = DBhelper.getDatatableFromSql(m_ologin.UfDbName, "select * from fitemss" + dr["citem_class"].ToString());
                foreach (DataRow drItemss in dtItemss.Rows)
                {
                    XmlElement xmlItemss = xmlDoc.CreateElement("item");
                    foreach (DataColumn dc in dtItemss.Columns)
                    {
                        XmlAttribute att = xmlDoc.CreateAttribute(dc.ColumnName);
                        att.Value = drItemss[dc.ColumnName].ToString();
                        xmlItemss.Attributes.Append(att);
                    }
                    xmlFitem.SelectSingleNode("//itemclass[@citemccode='" + drItemss["citemccode"].ToString() + "']").AppendChild(xmlItemss);
                }
                #endregion

                xmldata.AppendChild(xmlFitem);
            }




            xmlDoc.AppendChild(xmldata);
            strResult = xmldata.OuterXml;
            //xmlDoc.Save(HttpContext.Current.Server.MapPath(".") + "\\Temp\\fitem.xml");
            return strResult;
        }
    }
}