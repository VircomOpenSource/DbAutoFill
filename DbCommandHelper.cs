using DatabaseAutoFill.Types;
using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace DatabaseAutoFill
{
    public abstract class DbCommandHelper<TDbConnection>
        where TDbConnection : IDbConnection, new()
    {
        private string _baseCmd { get; set; }
        private string _connString { get; set; }

        protected abstract string CreateBaseCommandString(string connectionString, string schemaName);

        public DbCommandHelper(string connectionString)
            : this(connectionString, null)
        {

        }

        /// <exception cref="ArgumentException"></exception>
        public DbCommandHelper(string connectionString, string schemaName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection String must not be empty.", "connectionString");

            _connString = connectionString;
            _baseCmd = CreateBaseCommandString(connectionString, schemaName);

            if (string.IsNullOrWhiteSpace(_baseCmd))
                throw new FormatException(string.Format("Base command string was invalid for given object {0}.", this.GetType().AssemblyQualifiedName));

            using (IDbConnection conn = new TDbConnection())
            {
                conn.ConnectionString = _connString;
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Connection string appears to be invalid: couldn't open connection.", ex);
                }
            }
        }

        /// <summary>
        /// Takes the caller's name and tries to find a stored procedure named identically on the database and execute it
        /// from the InputType content as parameter and returns a TResultType object filled with results.
        /// 
        /// This function is guaranteed to not throw, and to always wrap errors in the DbResponse object.
        /// </summary>
        /// <typeparam name="TInputType"></typeparam>
        /// <typeparam name="TResultType">Returning object type. That object will be filled with returning data from database.</typeparam>
        /// <param name="inputObject">Object type that contains data to be sent to the stored procedure.</param>
        /// <param name="callerName">If not filled, will be replaced by the calling function's name.</param>
        /// <returns>DatabaseResponse containing a list of TResultType objects or the error message.</returns>
        public DbResponse<TResultType> ExecuteDbProcedureNamedAsCallerName<TInputType, TResultType>(TInputType inputObject, [CallerMemberName] string callerName = "")
            where TResultType : new()
        {
            if (inputObject == null)
                return new DbResponse<TResultType>(string.Format("Argument inputObject must not be null. Caller: {0}", callerName), new ArgumentNullException("inputObject"));

            string procedureName = string.Format(_baseCmd, callerName);
            DbResponse<TResultType> response = new DbResponse<TResultType>();

            using (TDbConnection conn = new TDbConnection())
            {
                conn.ConnectionString = _connString;
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    return new DbResponse<TResultType>(string.Format("Couldn't open connection to database. Caller: {0}", callerName), ex);
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = procedureName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        if (inputObject != null && !(inputObject is DBNull))
                        {
                            if (inputObject is IDbAnonymousValue)
                            {
                                IDbAnonymousValue param = inputObject as IDbAnonymousValue;
                                DbAutoFillHelper.AddParameterWithValue(cmd, param.Alias, param.GetValue(), null);
                            }
                            else
                                DbAutoFillHelper.AddParametersFromObjectMembers<TInputType>(cmd, inputObject);
                        }
                        IDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            TResultType obj = new TResultType();
                            DbAutoFillHelper.FillObjectFromDataReader<TResultType>(obj, dr);
                            response.Add(obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        return new DbResponse<TResultType>(string.Format("An error occured while retrieving data for caller {0}. Database command: '{1}'. Error: {2}", callerName, procedureName, ex.Message), ex);
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Takes the caller's name and tries to find a stored procedure named identically on the database and execute it without parameters.
        /// 
        /// This function is guaranteed to not throw, and to always wrap errors in the DbResponse object.
        /// </summary>
        /// <typeparam name="TResultType">Returning object type. That object will be filled with returning data from database.</typeparam>
        /// <param name="inputObject">Object type that contains data to be sent to the stored procedure.</param>
        /// <param name="callerName">If not filled, will be replaced by the caller function's name.</param>
        /// <returns>DatabaseResponse containing a list of TResultType objects or the error message.</returns>
        public DbResponse<TResultType> ExecuteDbProcedureNamedAsCallerName<TResultType>([CallerMemberName] string callerName = "")
            where TResultType : new()
        {
            return ExecuteDbProcedureNamedAsCallerName<DBNull, TResultType>(DBNull.Value, callerName);
        }
    }
}
