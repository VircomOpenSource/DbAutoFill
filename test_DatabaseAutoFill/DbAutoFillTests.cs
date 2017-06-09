using DatabaseAutoFill;
using DatabaseAutoFill.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SqlClient;

namespace test_DatabaseAutoFill
{
    [TestClass]
    public class DbAutoFillTests
    {
        #region --CONSTANTS--
        private const int DEFAULT_HIDDEN_PARAMETER_VALUE = 50;
        private const int DEFAULT_PARAMETER_2_VALUE = 100;
        private const string DEFAULT_PARAMETER_1_VALUE = "I am working";
        private const int DEFAULT_NONEPARAMETER_VALUE = 120;
        private const int DEFAULT_ToDBPARAMETER_VALUE = 200;
        private const int DEFAULT_FROMDBPARAMETER_VALUE = 300;

        private const string DEFAULT_PARAMETER1_NAME = "Parameter1";
        private const string DEFAULT_PARAMETER2_NAME = "Parameter2";
        private const string DEFAULT_HIDDEN_PARAMETER_NAME = "HiddenParameter";
        private const string DEFAULT_NONE_PARAMETER_NAME = "NoneParameter";
        private const string DEFAULT_ToDB_PARAMETER_NAME = "ToDBParameter";
        private const string DEFAULT_FROMDB_PARAMETER_NAME = "FromDBParameter";

        private const int DATAREADER_HIDDEN_PARAMETER_VALUE = 600;
        private const int DATAREADER_PARAMETER_2_VALUE = 700;
        private const string DATAREADER_PARAMETER_1_VALUE = "I am working my friend.";
        private const int DATAREADER_NONEPARAMETER_VALUE = 3420;
        private const int DATAREADER_ToDBPARAMETER_VALUE = 11200;
        private const int DATAREADER_FROMDBPARAMETER_VALUE = 5645300;

        private const string ADVANCED_PARAMETER_PREFIX_OVERRIDE_PARAM_NAME = "@param_ParameterPrefixOverride";
        private const string ADVANCED_PARAMETER_PREFIX_OVERRIDE_NAME = "ParameterPrefixOverride";
        private const string ADVANCED_PARAMETER_ALLOWED_MISSING_PARAM_NAME = "@p_ParameterAllowedMissing";
        private const string ADVANCED_PARAMETER_ALLOWED_MISSING_NAME = "ParameterAllowedMissing";
        private const string ADVANCED_PARAMETER_ALIASED_PARAM_NAME = "@p_MyParam";
        private const string ADVANCED_PARAMETER_ALIASED_NAME = "MyParam";
        private const string ADVANCED_PARAMETER_DbTypeDT_PARAM_NAME = "@p_DbTypeParameter";
        private const string ADVANCED_PARAMETER_DbTypeDT_NAME = "DbTypeParameter";
        private const string ADVANCED_PARAMETER_DbTypeDTDefault_PARAM_NAME = "@p_DbTypeParameterDefault";
        private const string ADVANCED_PARAMETER_DbTypeDTDefault_NAME = "DbTypeParameterDefault";
        private const int ADVANCED_DATAREADER_ALIASED_VALUE = 5555;
        private static readonly DateTime ADVANCED_DATAREADER_DBTYPEDEFAULT_VALUE = DateTime.Parse("2017/05/05 5:00 AM");
        private static readonly DateTime ADVANCED_DATAREADER_DBTYPE_VALUE = DateTime.Parse("2016/12/12 6:00 PM");
        private const string ADVANCED_DATAREADER_PREFIX_VALUE = "Hey, Sexy Lib!";

        private const string ADVANCED_PARAMETER_SUFFIX_PARAM_NAME = "@p_WithSuffix_IN";
        #endregion

        #region --OBJECTS--
        [DbAutoFill]
        public class DefaultValuesClassObject
        {
            public string Parameter1 { get; set; }
            public int Parameter2 { get; set; }
            private int HiddenParameter { get; set; }

            [DbAutoFill(FillBehavior = FillBehavior.None)]
            public int NoneParameter { get; set; }

            [DbAutoFill(FillBehavior = FillBehavior.ToDB)]
            public int ToDBParameter { get; set; }

            [DbAutoFill(FillBehavior = FillBehavior.FromDB)]
            public int FromDBParameter { get; set; }

            public DefaultValuesClassObject()
            {
                Parameter1 = DEFAULT_PARAMETER_1_VALUE;
                Parameter2 = DEFAULT_PARAMETER_2_VALUE;
                ToDBParameter = DEFAULT_ToDBPARAMETER_VALUE;
                HiddenParameter = DEFAULT_HIDDEN_PARAMETER_VALUE;
                FromDBParameter = DEFAULT_FROMDBPARAMETER_VALUE;
                NoneParameter = DEFAULT_NONEPARAMETER_VALUE;
            }
        }

        [DbAutoFill(ParameterPrefix = "@p_")]
        public class AdvancedAutoFillClass
        {
            [DbAutoFill(ParameterPrefix = "@param_")]
            public string ParameterPrefixOverride { get; set; }

            [DbAutoFill(AllowMissing = true)]
            public int ParameterAllowedMissing { get; set; }

            [DbAutoFill(Alias = "MyParam")]
            public int AliasedParameter { get; set; }

            [DbAutoFill(DbType = DbType.DateTime2)]
            public DateTime DbTypeParameter { get; set; }

            [DbAutoFill(DbType = DbType.DateTime)]
            public DateTime DbTypeParameterDefault { get; set; }

            [DbAutoFill(ParameterSuffix = "_IN", FillBehavior = FillBehavior.ToDB)]
            public int WithSuffix { get; set; }
        }

        [DbAutoFill(ParameterPrefix = "@p_", ParameterSuffix = "_IN")]
        public class ClassWithSuffix
        {
            public int WithSuffix { get; set; }
        }

        #endregion

        private SqlCommand _command;
        private DefaultValuesClassObject _dvco;
        private IDataReader _dataReader;

        [TestInitialize]
        public void Setup()
        {
            _command = new SqlCommand();
            _dvco = new DefaultValuesClassObject();
            _dataReader = CreateDataReader();
            _dataReader.Read();
        }

        [TestCleanup]
        public void Teardown()
        {
            _command.Dispose();
            _dvco = null;
            _dataReader.Dispose();
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void Basic_ResultSetAlwaysSet_Default_Test()
        {
            DbResponse<bool> response = new DbResponse<bool>();
            Assert.IsNotNull(response.ResultSet);
            Assert.IsFalse(response.HasResult);
            Assert.IsFalse(response.HasError);
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void Basic_ResultSetAlwaysSet_Error_Test()
        {
            DbResponse<bool> response = new DbResponse<bool>("Bad error", new Exception("Error"));
            Assert.IsTrue(response.HasError);
            Assert.IsNotNull(response.Exception);
            Assert.IsFalse(string.IsNullOrWhiteSpace(response.ErrorMessage));
            Assert.IsNotNull(response.ResultSet);
            Assert.IsFalse(response.HasResult);
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void Advanced_SetParametersFromObject_GoodParameters_Test()
        {
            AdvancedAutoFillClass aafc = new AdvancedAutoFillClass();
            DbAutoFillHelper.AddParametersFromObjectMembers(_command, aafc);

            Assert.IsTrue(_command.Parameters.Count > 0);
            Assert.IsTrue(_command.Parameters.Contains(ADVANCED_PARAMETER_ALIASED_PARAM_NAME));
            Assert.IsTrue(_command.Parameters.Contains(ADVANCED_PARAMETER_ALLOWED_MISSING_PARAM_NAME));
            Assert.IsTrue(_command.Parameters.Contains(ADVANCED_PARAMETER_DbTypeDTDefault_PARAM_NAME));
            Assert.IsTrue(_command.Parameters.Contains(ADVANCED_PARAMETER_DbTypeDT_PARAM_NAME));
            Assert.IsTrue(_command.Parameters.Contains(ADVANCED_PARAMETER_PREFIX_OVERRIDE_PARAM_NAME));
        }

        [TestMethod]
        public void Advanced_SetParametersFromObject_ParametersSuffix_OnProperty_Test()
        {
            AdvancedAutoFillClass aafc = new AdvancedAutoFillClass();
            DbAutoFillHelper.AddParametersFromObjectMembers(_command, aafc);

            Assert.IsTrue(_command.Parameters.Contains(ADVANCED_PARAMETER_SUFFIX_PARAM_NAME));
        }

        [TestMethod]
        public void Advanced_SetParametersFromObject_ParametersSuffix_OnClass_Test()
        {
            ClassWithSuffix cws = new ClassWithSuffix();
            DbAutoFillHelper.AddParametersFromObjectMembers(_command, cws);

            Assert.IsTrue(_command.Parameters.Contains(ADVANCED_PARAMETER_SUFFIX_PARAM_NAME));
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void Advanced_GetParametersFromDataReader_GoodValues_Test()
        {
            AdvancedAutoFillClass aafc = new AdvancedAutoFillClass();
            IDataReader reader = CreateAdvancedDataReader();
            reader.Read();
            DbAutoFillHelper.FillObjectFromDataReader(aafc, reader);

            Assert.AreEqual(aafc.AliasedParameter, ADVANCED_DATAREADER_ALIASED_VALUE);
            Assert.AreEqual(aafc.DbTypeParameter, ADVANCED_DATAREADER_DBTYPE_VALUE);
            Assert.AreEqual(aafc.DbTypeParameterDefault, ADVANCED_DATAREADER_DBTYPEDEFAULT_VALUE);
            Assert.AreEqual(aafc.ParameterPrefixOverride, ADVANCED_DATAREADER_PREFIX_VALUE);
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void SetParametersFromObject_GoodParameters_Test()
        {
            DbAutoFillHelper.AddParametersFromObjectMembers(_command, _dvco);

            Assert.IsTrue(_command.Parameters.Contains(DEFAULT_PARAMETER1_NAME));
            Assert.IsTrue(_command.Parameters.Contains(DEFAULT_PARAMETER2_NAME));
            Assert.IsFalse(_command.Parameters.Contains(DEFAULT_HIDDEN_PARAMETER_NAME));
            Assert.IsTrue(_command.Parameters.Contains(DEFAULT_ToDB_PARAMETER_NAME));
            Assert.IsFalse(_command.Parameters.Contains(DEFAULT_NONE_PARAMETER_NAME));
            Assert.IsFalse(_command.Parameters.Contains(DEFAULT_FROMDB_PARAMETER_NAME));
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void SetParametersFromObject_GoodValues_Test()
        {
            DbAutoFillHelper.AddParametersFromObjectMembers(_command, _dvco);

            Assert.AreEqual(_command.Parameters[DEFAULT_PARAMETER1_NAME].Value, DEFAULT_PARAMETER_1_VALUE);
            Assert.AreEqual(_command.Parameters[DEFAULT_PARAMETER2_NAME].Value, DEFAULT_PARAMETER_2_VALUE);
            Assert.AreEqual(_command.Parameters[DEFAULT_ToDB_PARAMETER_NAME].Value, DEFAULT_ToDBPARAMETER_VALUE);
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void FillObjectFromDataReader_GoodValues_Test()
        {
            _dvco = new DefaultValuesClassObject();

            DbAutoFillHelper.FillObjectFromDataReader(_dvco, _dataReader);

            Assert.AreEqual(_dvco.Parameter1, DATAREADER_PARAMETER_1_VALUE);
            Assert.AreEqual(_dvco.Parameter2, DATAREADER_PARAMETER_2_VALUE);
            Assert.AreEqual(_dvco.FromDBParameter, DATAREADER_FROMDBPARAMETER_VALUE);
            Assert.AreNotEqual(_dvco.NoneParameter, DATAREADER_NONEPARAMETER_VALUE);
            Assert.AreNotEqual(_dvco.ToDBParameter, DATAREADER_ToDBPARAMETER_VALUE);

        }

        private DataTable CreateNewDataTable()
        {
            DataTable dt = new DataTable("DefaultParametersResultSet");
            dt.Columns.Add(DEFAULT_PARAMETER1_NAME, typeof(string));
            dt.Columns.Add(DEFAULT_PARAMETER2_NAME, typeof(int));
            dt.Columns.Add(DEFAULT_HIDDEN_PARAMETER_NAME, typeof(int));
            dt.Columns.Add(DEFAULT_NONE_PARAMETER_NAME, typeof(int));
            dt.Columns.Add(DEFAULT_ToDB_PARAMETER_NAME, typeof(int));
            dt.Columns.Add(DEFAULT_FROMDB_PARAMETER_NAME, typeof(int));
            dt.AcceptChanges();
            return dt;
        }

        private DataTable CreateAdvancedDataTable()
        {
            DataTable dt = new DataTable("AdvancedParametersResultSet");
            dt.Columns.Add(ADVANCED_PARAMETER_ALIASED_NAME, typeof(int));
            dt.Columns.Add(ADVANCED_PARAMETER_DbTypeDTDefault_NAME, typeof(DateTime));
            dt.Columns.Add(ADVANCED_PARAMETER_DbTypeDT_NAME, typeof(DateTime));
            dt.Columns.Add(ADVANCED_PARAMETER_PREFIX_OVERRIDE_NAME, typeof(string));
            dt.AcceptChanges();
            return dt;
        }

        private IDataReader CreateAdvancedDataReader()
        {
            DataTable dt = CreateAdvancedDataTable();
            dt.Rows.Add(ADVANCED_DATAREADER_ALIASED_VALUE,
                ADVANCED_DATAREADER_DBTYPEDEFAULT_VALUE,
                ADVANCED_DATAREADER_DBTYPE_VALUE,
                ADVANCED_DATAREADER_PREFIX_VALUE);
            dt.AcceptChanges();
            return dt.CreateDataReader();
        }

        private IDataReader CreateDataReader()
        {
            DataTable dt = CreateNewDataTable();
            dt.Rows.Add(DATAREADER_PARAMETER_1_VALUE,
                DATAREADER_PARAMETER_2_VALUE,
                DATAREADER_HIDDEN_PARAMETER_VALUE,
                DATAREADER_NONEPARAMETER_VALUE,
                DATAREADER_ToDBPARAMETER_VALUE,
                DATAREADER_FROMDBPARAMETER_VALUE);
            dt.AcceptChanges();
            return dt.CreateDataReader();
        }
    }
}
