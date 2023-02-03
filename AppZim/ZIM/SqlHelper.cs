using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WebUI.Business
{
    public class SqlHelper
    {
        #region ConnectionString

        static string GetConnection
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["Zim"].ConnectionString;
            }
        }

        #endregion

        /// <summary>
        /// Execute an update store
        /// </summary>
        /// <param name="storeName">Store procedure name</param>
        /// <param name="pars">Sql Parameter command</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string storeName, SqlParameter[] pars)
        {
            var connection = new SqlConnection { ConnectionString = GetConnection };
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                return string.IsNullOrEmpty(storeName) ? 0 : Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(GetConnection, CommandType.StoredProcedure, storeName, pars);
            }
            catch { transaction.Rollback(); throw; }
            finally { if (connection.State == ConnectionState.Open) connection.Close(); }

        }

        /// <summary>
        /// Execute an command text
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string commandText)
        {
            var connection = new SqlConnection { ConnectionString = GetConnection };
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                return string.IsNullOrEmpty(commandText) ? 0 : Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(GetConnection, CommandType.Text, commandText);
            }
            catch { transaction.Rollback(); throw; }
            finally { if (connection.State == ConnectionState.Open) connection.Close(); }

        }

        /// <summary>
        /// Get DataTable by Store Procedure
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string storeName, SqlParameter[] pars)
        {
            var connection = new SqlConnection { ConnectionString = GetConnection };
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteDataset(GetConnection, CommandType.StoredProcedure, storeName, pars).Tables[0];
            }
            catch { transaction.Rollback(); throw; }
            finally { if (connection.State == ConnectionState.Open) connection.Close(); }
        }

        /// <summary>
        /// Get DataTable by command text
        /// </summary>
        /// <param name="commandLine"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string commandText)
        {
            var connection = new SqlConnection { ConnectionString = GetConnection };
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteDataset(GetConnection, CommandType.Text, commandText).Tables[0];
            }
            catch { transaction.Rollback(); throw; }
            finally { if (connection.State == ConnectionState.Open) connection.Close(); }
        }

        /// <summary>
        /// Get as SqlDataReader
        /// </summary>
        /// <param name="commandText">command text</param>
        /// <returns></returns>
        public static SqlDataReader ExecuteDataReader(string commandText)
        {
            var connection = new SqlConnection { ConnectionString = GetConnection };
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteReader(GetConnection, CommandType.Text, commandText);
            }
            catch { transaction.Rollback(); throw; }
            finally { if (connection.State == ConnectionState.Open) connection.Close(); }

        }

        /// <summary>
        /// Get as SqlDataReader
        /// </summary>
        /// <param name="storeName">store name</param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public static SqlDataReader ExecuteDataReader(string storeName, SqlParameter[] pars)
        {
            var connection = new SqlConnection { ConnectionString = GetConnection };
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteReader(GetConnection, CommandType.StoredProcedure, storeName, pars);
            }
            catch { transaction.Rollback(); throw; }
            finally { if (connection.State == ConnectionState.Open) connection.Close(); }

        }

        /// <summary>
        /// Get as Object
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string storeName, SqlParameter[] pars)
        {
            var connection = new SqlConnection { ConnectionString = GetConnection };
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteScalar(GetConnection, CommandType.StoredProcedure, storeName, pars);
            }
            catch { transaction.Rollback(); throw; }
            finally { if (connection.State == ConnectionState.Open) connection.Close(); }


        }

        /// <summary>
        /// Get as object
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string commandText)
        {
            var connection = new SqlConnection { ConnectionString = GetConnection };
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                return Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteScalar(GetConnection, CommandType.Text, commandText);
            }
            catch { transaction.Rollback(); throw; }
            finally { if (connection.State == ConnectionState.Open) connection.Close(); }


        }
    }
}