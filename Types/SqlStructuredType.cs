using Microsoft.SqlServer.Server;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DatabaseAutoFill.Types
{
    /// <summary>
    /// SqlStructuredType for MSSQL
    /// </summary>
    /// <typeparam name="TData">Inner CLR datatype</typeparam>
    public abstract class SqlStructuredType<TData> : DbStructuredType<TData, SqlDataRecord>
    {
        public override void SetParameterValue(IDbDataParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");

            if (!(parameter is SqlParameter))
                throw new ArgumentException("Parameter must be of SqlParameter type for SqlStructuredType.", "parameter");

            SqlParameter p = parameter as SqlParameter;

            p.Value = this;
            p.SqlDbType = SqlDbType.Structured;
        }
    }
}
