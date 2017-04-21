using DatabaseAutoFill;
using DatabaseAutoFill.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.SqlClient;

namespace test_DatabaseAutoFill
{
    [TestClass]
    public class DbAnonymousValueTests
    {
        private const string ANONYMOUS_VALUE_ALIAS = "MyAnonymousAlias";
        private const int ANONYMOUS_SET_VALUE = 90;

        private SqlCommand _command;
        private IDataReader _dataReader;

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void SetParameters_NamedDbAnonymousValue_Test()
        {
            DbAnonymousValue<int> val = new DbAnonymousValue<int>(ANONYMOUS_VALUE_ALIAS, ANONYMOUS_SET_VALUE);
            DbAutoFillHelper.AddParameterWithValue(_command, val);

            Assert.IsTrue(_command.Parameters.Contains(ANONYMOUS_VALUE_ALIAS));
            Assert.AreEqual(_command.Parameters[ANONYMOUS_VALUE_ALIAS].Value, ANONYMOUS_SET_VALUE);
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void SetParameters_NamedAnonymous_Raw_Test()
        {
            DbAutoFillHelper.AddParameterWithValue(_command, ANONYMOUS_VALUE_ALIAS, ANONYMOUS_SET_VALUE, null);
            Assert.IsTrue(_command.Parameters.Contains(ANONYMOUS_VALUE_ALIAS));
            Assert.AreEqual(_command.Parameters[ANONYMOUS_VALUE_ALIAS].Value, ANONYMOUS_SET_VALUE);
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void GetDbAnonymousValue_Test()
        {
            DbAnonymousValue<int> value = new DbAnonymousValue<int>();
            DbAutoFillHelper.FillObjectFromDataReader(value, _dataReader);

            Assert.AreEqual(value.GetValue(), ANONYMOUS_SET_VALUE);
        }

        [TestInitialize]
        public void Setup()
        {
            _command = new SqlCommand();
            _dataReader = CreateDataReader();
            _dataReader.Read();
        }

        [TestCleanup]
        public void Teardown()
        {
            _command.Dispose();
            _dataReader.Dispose();
        }

        private DataTable CreateNewDataTable()
        {
            DataTable dt = new DataTable("DbAnonymousValueResultSets");
            dt.Columns.Add(ANONYMOUS_VALUE_ALIAS, typeof(int));
            dt.AcceptChanges();
            return dt;
        }

        private IDataReader CreateDataReader()
        {
            DataTable dt = CreateNewDataTable();
            dt.Rows.Add(ANONYMOUS_SET_VALUE);
            dt.AcceptChanges();
            return dt.CreateDataReader();
        }

    }
}
