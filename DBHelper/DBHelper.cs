using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Data.SQLite;
using MySql.Data.MySqlClient;
using System.Text;

namespace KtsDBHelper
{

    public enum DBType
    {
        /// <summary>
        /// SQL Server数据库
        /// </summary>
        SQLServer,
        /// <summary>
        /// Access类型数据库  string conStr = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=E:\数据库\XiaoZhen.mdb;"
        /// </summary>
        OleDb,
        /// <summary>
        /// ODBC数据源   string conStr = "DSN=XXX;uid=sa;pwd=sa;"
        /// </summary>
        ODBC,
        /// <summary>
        /// Oracle数据库
        /// </summary>
        Oracle,
        /// <summary>
        /// MySQL
        /// </summary>
        MySQL,
        /// <summary>
        /// SQLite Data Source={0};Pooling=true; FailIfMissing=false；
        /// </summary>
        SQLite
    }
     
    /// <summary>
    /// 2014年4月22日18:22:28 思信软件 崔
    /// 通用数据库访问类 DBHelper类，
    /// 支持SQL Server、OleDb、ODBC、Oracle、MySQL、SQLite等
    /// 不同类型的数据源
    /// </summary>
    public class DBHelper : IDisposable
    {
        /// <summary>
        /// 该枚举类型用于枚举数据库的类型
        /// </summary>


        #region 初始化数据库工厂
        private DBType _DatabaseType = DBType.SQLServer;
        private DbProviderFactory provider = null;
        /// <summary>
        /// 获取数据库类型属性
        /// </summary>
        public DBType DatabaseType
        {
            get { return _DatabaseType; }
        }

        private string _ConnectionString = "";
        /// <summary>
        /// 获取数据库连接字符串属性
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString; }
        }
        /// <summary>
        /// 构造函数，默认数据库是SQL Server
        /// </summary>
        public DBHelper(string ConString)
        {
            this._DatabaseType = DBType.SQLServer;
            this._ConnectionString = ConString;
            GetProvider();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ConString">数据库连接字符串</param> 
        /// <param name="DbType">访问的数据库类型</param>
        public DBHelper(string ConString, DBType DbType)
        {
            this._DatabaseType = DbType;
            this._ConnectionString = ConString;
            GetProvider();
        }


        /// <summary>
        /// 根据数据库类型获取数据库实例
        /// </summary>
        /// <returns></returns>
        private void GetProvider()
        {
            switch (this.DatabaseType)
            {
                case DBType.SQLServer:
                    provider = SqlClientFactory.Instance;
                    break;
                case DBType.OleDb:
                    provider = OleDbFactory.Instance;
                    break;
                case DBType.ODBC:
                    provider = OdbcFactory.Instance;
                    break;
                case DBType.Oracle:
                    provider = OracleClientFactory.Instance;
                    break;
                case DBType.MySQL:
                    provider = MySqlClientFactory.Instance;
                    break;
                case DBType.SQLite:
                    provider = SQLiteFactory.Instance;
                    break;
                default:
                    provider = SqlClientFactory.Instance;
                    break;
            }
        }
        #endregion
         
        #region  ExecuteNonQuery
        /// <summary>
        /// 执行SQL语句并返回受影响的行的数目
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>受影响的行的数目</returns>
        public int ExecuteNonQuery(string cmdText)
        {
            return ExecuteNonQuery(cmdText, CommandType.Text, null);
        }

        /// <summary>
        /// 执行SQL语句并返回受影响的行的数目
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>受影响的行的数目</returns>
        public int ExecuteNonQuery(string cmdText, CommandType cmdType)
        {
            return ExecuteNonQuery(cmdText, cmdType, null);
        }

        /// <summary>
        /// 执行SQL语句并返回受影响的行的数目
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>受影响的行的数目</returns>
        public int ExecuteNonQuery(string cmdText, Dictionary<string, object> ParametersList)
        {
            return ExecuteNonQuery(cmdText, CommandType.Text, ParametersList);
        }


        /// <summary>
        /// 执行SQL语句并返回受影响的行的数目
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>受影响的行的数目</returns>
        public int ExecuteNonQuery(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {
            int val = -1;
            using (DbConnection conn = provider.CreateConnection())
            {
                var cmd = conn.CreateCommand();
                PrepareCommand(conn, cmd, cmdText, cmdType, ParametersList);
                try
                {
                    val = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    WriteExLog(ex, cmdText, ParametersList);
                    throw ex;
                }
            }
            return val;
        }

        #endregion

        #region ExecuteReader
        /// <summary>
        /// 执行SQL语句并返回数据行
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>数据读取器接口</returns>
        public IDataReader ExecuteReader(string cmdText)
        {
            return ExecuteReader(cmdText, CommandType.Text, null);
        }
        /// <summary>
        /// 执行SQL语句并返回数据行
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>数据读取器接口</returns>
        public IDataReader ExecuteReader(string cmdText, CommandType cmdType)
        {
            return ExecuteReader(cmdText, cmdType, null);
        }

        /// <summary>
        /// 执行SQL语句并返回数据行
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据读取器接口</returns>
        public IDataReader ExecuteReader(string cmdText, Dictionary<string, object> ParametersList)
        {
            return ExecuteReader(cmdText, CommandType.Text, ParametersList);
        }

        /// <summary>
        /// 执行SQL语句并返回数据行
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据读取器接口</returns>
        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {
            IDataReader reader = null;
            using (DbConnection conn = provider.CreateConnection())
            {
                var cmd = conn.CreateCommand();
                PrepareCommand(conn, cmd, cmdText, cmdType, ParametersList);
                try
                {
                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    WriteExLog(ex, cmdText, ParametersList);
                    throw ex;
                }
                return reader;
            }
        }

        #endregion

        #region GetOneResult
        /// <summary>
        /// 执行SQL语句并返回单值对象
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public object GetOneResult(string cmdText)
        {
            return GetOneResult(cmdText, CommandType.Text, null);
        }

        /// <summary>
        /// 执行SQL语句并返回单值
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public string GetOneString(string cmdText)
        {
            string str = "";
            var res = GetOneResult(cmdText, CommandType.Text, null);
            if (res != null) { str = res.ToString(); }
            return str;
        }
        /// <summary>
        /// 执行SQL语句并返回单值对象
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public object GetOneResult(string cmdText, CommandType cmdType)
        {
            return GetOneResult(cmdText, cmdType, null);
        }

        /// <summary>
        /// 执行SQL语句并返回单值
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public string GetOneString(string cmdText, CommandType cmdType)
        {
            string str = "";
            var res = GetOneResult(cmdText, cmdType, null);
            if (res != null) { str = res.ToString(); }
            return str;
        }

        /// <summary>
        /// 执行SQL语句并返回单值对象
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public object GetOneResult(string cmdText, Dictionary<string, object> ParametersList)
        {
            return GetOneResult(cmdText, CommandType.Text, ParametersList);
        }


        public object GetOneResult(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {
            object obj = null;
            using (DbConnection conn = provider.CreateConnection())
            {
                var cmd = conn.CreateCommand();
                PrepareCommand(conn, cmd, cmdText, cmdType, ParametersList);
                try
                {
                    obj = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    StringBuilder sblog = new StringBuilder();
                    sblog.Append(ex.Message + System.Environment.NewLine);
                    sblog.Append("CommandText: " + cmdText + System.Environment.NewLine);
                    if (ParametersList != null)
                    {
                        foreach (var item in ParametersList)
                        {
                            sblog.Append("Params: " + item.Key + " = " + item.Value.ToString() + System.Environment.NewLine);
                        }
                    }
                    KtsDBHelper.Log.WriteExLog(sblog.ToString(), typeof(DBHelper));
                    throw ex;
                }
                return obj;
            }
        }

        #endregion

        #region GetDataSet
        /// <summary>
        /// 填充一个数据集对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>数据集对象</returns>
        public DataSet GetDataSet(string cmdText)
        {
            return GetDataSet(cmdText, CommandType.Text, null);
        }
        /// <summary>
        /// 填充一个数据集对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>数据集对象</returns>
        public DataSet GetDataSet(string cmdText, CommandType cmdType)
        {
            return GetDataSet(cmdText, cmdType, null);
        }
        /// <summary>
        /// 填充一个数据集对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据集对象</returns>
        public DataSet GetDataSet(string cmdText, Dictionary<string, object> ParametersList)
        {
            return GetDataSet(cmdText, CommandType.Text, ParametersList);
        }
        /// <summary>
        /// 填充一个数据集对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据集对象</returns>
        private DataSet GetDataSet(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {
            DataSet ds = new DataSet();
            using (DbConnection conn = provider.CreateConnection())
            {
                var cmd = conn.CreateCommand();
                PrepareCommand(conn, cmd, cmdText, cmdType, ParametersList);
                DbDataAdapter da = provider.CreateDataAdapter();
                da.SelectCommand = cmd;
                try
                {
                    da.Fill(ds);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return ds;
            }
        }

        #endregion

        #region GetDataTable
        /// <summary>
        /// 填充一个数据表对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>数据表对象</returns>
        public DataTable GetDataTable(string cmdText)
        {
            return GetDataTable(cmdText, CommandType.Text, null);
        }

        /// <summary>
        /// 填充一个数据表对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>数据表对象</returns>
        public DataTable GetDataTable(string cmdText, CommandType cmdType)
        {
            return GetDataTable(cmdText, cmdType, null);
        }

        /// <summary>
        /// 填充一个数据表对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据表对象</returns>
        public DataTable GetDataTable(string cmdText, Dictionary<string, object> ParametersList)
        {
            return GetDataTable(cmdText, CommandType.Text, ParametersList);
        }
        /// <summary>
        /// 填充一个数据表对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据表对象</returns>
        public DataTable GetDataTable(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {
            DataTable dt = new DataTable();
            using (DbConnection conn = provider.CreateConnection())
            {
                var cmd = conn.CreateCommand();
                PrepareCommand(conn, cmd, cmdText, cmdType, ParametersList);
                DbDataAdapter da = provider.CreateDataAdapter();
                da.SelectCommand = cmd;
                try
                {
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return dt;
            }
        }

        #endregion

        #region GetOneDataRow
        /// <summary>
        /// 填充一个DataRow对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>数据表对象</returns>
        public DataRow GetOneDataRow(string cmdText)
        {
            var dt = GetDataTable(cmdText, CommandType.Text, null);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }
        /// <summary>
        /// 填充一个DataRow对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>数据表对象</returns>
        public DataRow GetOneDataRow(string cmdText, CommandType cmdType)
        {
            var dt = GetDataTable(cmdText, cmdType, null);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }
        /// <summary>
        /// 填充一个DataRow对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">ParametersList</param>
        /// <returns>数据表对象</returns>
        public DataRow GetOneDataRow(string cmdText, Dictionary<string, object> ParametersList)
        {
            var dt = GetDataTable(cmdText, CommandType.Text, ParametersList);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }
        /// <summary>
        /// 填充一个DataRow对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <param name="ParametersList">ParametersList</param>
        /// <returns>数据表对象</returns>
        public DataRow GetOneDataRow(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {
            var dt = GetDataTable(cmdText, cmdType, ParametersList);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }
        #endregion

        #region ExecuteProcedure
        /// <summary>
        /// 执行带返回参数的存储过程
        /// </summary>
        /// <param name="ProcedureName">存储过程名</param>
        /// <param name="spl">参数列表</param>
        /// <returns></returns>
        public Dictionary<string, object> ExecuteProcedure(string ProcedureName, DbParameter[] ParametersList)
        { 
            Dictionary<string, object> dc = new Dictionary<string, object>(); 
            using (DbConnection conn = provider.CreateConnection())
            {
                try
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = ProcedureName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(ParametersList);
                    cmd.ExecuteNonQuery();
                    foreach (var item in ParametersList)
                    {
                        if (item.Direction == ParameterDirection.Output)
                        {
                            dc.Add(item.ParameterName, cmd.Parameters[item.ParameterName].Value);
                        }

                    }
                    return dc;
                }
                catch (Exception ex)
                {
                    WriteExLog(ex, ProcedureName, ParametersList);
                    throw ex;
                }

            }
        }
         
        #endregion
         
        /// <summary>
        /// 执行多条Sql命令(包含事務功能)
        /// </summary>
        /// <param name="cmdTextList">Sql命令数组</param>
        /// <returns>正确执行返回True，错误执行为False</returns>
        public bool ExecuteSqlList(List<string> cmdTextList)
        {
            bool flag = false;
            using (DbConnection conn = provider.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                var tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    foreach (string sql in cmdTextList)
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                    flag = true;
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    KtsDBHelper.Log.WriteExLog(ex.Message.ToString(), typeof(DBHelper));
                    throw ex;
                }
                return flag;
            }
        }

        private List<KeyValuePair<string,DbParameter[]>> TransactionList;
        public void AddToTransaction(string cmdText, Dictionary<string, object> ParametersList)
        {
            if (TransactionList == null)
            { TransactionList = new List<KeyValuePair<string,DbParameter[]>>(); }
            List<DbParameter> LP = new List<DbParameter>();
            if (ParametersList != null)
            {
                foreach (var item in ParametersList)
                {
                    var param = provider.CreateParameter();
                    param.ParameterName = item.Key;
                    param.Value = item.Value;
                    LP.Add(param);
                }
            }
            
            DbParameter[] dbp= null;
            if (LP.Count > 0)
            {
                dbp = LP.ToArray();
            }
            KeyValuePair<string, DbParameter[]> kvp = new KeyValuePair<string, DbParameter[]>(
                cmdText,dbp
                );
            TransactionList.Add(kvp);  
        }


        /// <summary>
                ///  ExecuteTransaction 需要先AddToTransaction
        /// </summary>
        /// <returns></returns>
        public bool ExecuteTransaction()
        {
            bool flag = false;
            if (TransactionList == null || TransactionList.Count == 0)
            {
                return false;
            }
            using (DbConnection conn = provider.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                var tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {  
                    foreach (var item in TransactionList)
                    {
                        cmd.CommandText = item.Key;
                        cmd.Parameters.AddRange(item.Value);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    tx.Commit();
                    flag = true;
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    KtsDBHelper.Log.WriteExLog(ex.Message.ToString(), typeof(DBHelper));
                    throw ex;
                }
                finally {
                    TransactionList.Clear();
                }
                return flag;
            }
        }

        #region PrepareCommand
        /// <summary>
        /// 准备执行命令
        /// </summary>
        /// <param name="conn">DbConnection</param>
        /// <param name="cmd">DbCommand</param>
        /// <param name="cmdText">sql命令字符串</param>
        /// <param name="cmdType">执行方式</param>
        /// <param name="ParametersList">参数</param>
        private void PrepareCommand(DbConnection conn, DbCommand cmd, string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
            }

            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            if (ParametersList != null)
            {
                foreach (var p in ParametersList)
                {
                    var dp = cmd.CreateParameter();
                    dp.ParameterName = p.Key;
                    dp.Value = p.Value;
                    cmd.Parameters.Add(dp);
                }
            }
        }

        /// <summary>
        /// 准备执行一个命令
        /// </summary> 
        /// <param name="conn">OleDb连接</param>
        /// <param name="cmd">sql命令</param>
        /// <param name="trans">OleDb事务</param>
        /// <param name="cmdType">命令类型例如 存储过程或者文本</param>
        /// <param name="cmdText">命令文本,例如:Select * from Products</param>
        /// <param name="cmdParms">执行命令的参数</param>
        private void PrepareCommand(DbConnection conn, DbCommand cmd,
            SqlTransaction trans, CommandType cmdType,
            string cmdText, DbParameter[] cmdParms)
        {

            if (conn.State != ConnectionState.Open)
            { conn.Open(); }
            cmd.Parameters.Clear();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            if (trans != null)
            { cmd.Transaction = trans; }
            if (cmdParms != null)
            {
                foreach (var parm in cmdParms)
                {
                    cmd.Parameters.Add(parm);
                }
            }

        }
        #endregion


        private void WriteExLog(Exception ex, string cmdText, Dictionary<string, object> ParametersList)
        {
            StringBuilder sblog = new StringBuilder();
            sblog.Append(ex.Message + System.Environment.NewLine);
            sblog.Append("CommandText: " + cmdText + System.Environment.NewLine);
            if (ParametersList != null)
            {
                foreach (var item in ParametersList)
                {
                    sblog.Append("Params: " + item.Key + " = " + item.Value.ToString() + System.Environment.NewLine);
                }
            }
            KtsDBHelper.Log.WriteExLog(sblog.ToString(), typeof(DBHelper));
        }
        private void WriteExLog(Exception ex, string cmdText, DbParameter[] ParametersList)
        {
            StringBuilder sblog = new StringBuilder();
            sblog.Append(ex.Message + System.Environment.NewLine);
            sblog.Append("CommandText: " + cmdText + System.Environment.NewLine);
            if (ParametersList != null)
            {
                foreach (var item in ParametersList)
                {
                    sblog.Append("Params: " + item.ParameterName + " = " + item.Value.ToString() + System.Environment.NewLine);
                }
            }
            KtsDBHelper.Log.WriteExLog(sblog.ToString(), typeof(DBHelper));
        }

        #region  Dispose
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            //好像没啥可释放的了，conn用了using 。 
        }

        #endregion
    }
}
