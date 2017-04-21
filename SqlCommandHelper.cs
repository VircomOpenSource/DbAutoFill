using System.Data.SqlClient;
using System.Text;

namespace DatabaseAutoFill
{
    /// <summary>
    /// SQL Server implementation fo the DbCommandHelper.
    /// </summary>
    public class SqlCommandHelper : DbCommandHelper<SqlConnection>
    {
        public SqlCommandHelper(string connectionString)
            : base(connectionString, null)
        {

        }

        public SqlCommandHelper(string connectionString, string schemaName)
            : base(connectionString, schemaName)
        {
        }

        protected override string CreateBaseCommandString(string connectionString, string schemaName)
        {
            StringBuilder cmd = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(schemaName))
                cmd.AppendFormat("[{0}].", schemaName);

            cmd.Append("[{0}]");
            return cmd.ToString();
        }
    }
}
