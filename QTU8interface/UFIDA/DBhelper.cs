using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ADODB;
using System.Data;
using System.Data.OleDb;

namespace QTU8interface.UFIDA
{
    public static class DBhelper
    {
        public static string  getDataFromSql(string ConnStr,string Sql)
        {
            string DataResult="";
            ADODB.Connection conn=new Connection();
            conn.ConnectionString=ConnStr;
            ADODB.Recordset rst = new Recordset();
            try
            {
                conn.Open();
                rst.Open(Sql, conn, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockOptimistic);
                if (rst!=null)
                {
                    if (!rst.EOF)
                    {
                        //rst.MoveNext();
                        DataResult = rst.Fields[0].Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                DataResult = ex.Message;
            }
            finally
            {
                conn.Close();
            }
            return DataResult;
        }

        public static string setDataFromSql(string ConnStr,string Sql)
        {
            string DataResult = "";
            object objOut = null;
            ADODB.Connection conn = new Connection();
            try
            {                
                conn.ConnectionString = ConnStr;
                ADODB.Command cmd = new Command();
                conn.Open();
                cmd.ActiveConnection = conn;
                cmd.CommandType = CommandTypeEnum.adCmdText;
                cmd.CommandText = Sql;                
                DataResult = cmd.Execute(out objOut).ToString();
            }
            catch (Exception ex)
            {
                DataResult = ex.Message;
            }
            finally
            {
                conn.Close();
            }
            return DataResult;
        }

        public static DataTable getDatatableFromSql(string strConn, string strSql)//根据sql得到datatable
        {
            DataTable dtResult = null;
            OleDbConnection conn = new OleDbConnection(strConn);
            try
            {
                DataSet ds = new DataSet();
                conn.Open();
                OleDbDataAdapter adapter = new OleDbDataAdapter(strSql, conn);
                adapter.Fill(ds);
                dtResult = ds.Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(DBhelper), ex);
            }
            finally { conn.Close(); }
            return dtResult;
        }

        public static decimal Round(decimal d1)//四舍五入
        {
            return Math.Round(d1,2, MidpointRounding.AwayFromZero);
        }
    }
}