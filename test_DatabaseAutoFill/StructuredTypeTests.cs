using DatabaseAutoFill;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using test_DatabaseAutoFill.Objects;

namespace test_DatabaseAutoFill
{
    [TestClass]
    public class StructuredTypeTests
    {
        #region --CONSTANTS--
        private const string OBJECT_MYSTRUCTUREDFIELD_NAME = "MyStructuredField";
        private const string OBJECT_FILL_ELEMENT1_ID = "10";
        private const string OBJECT_FILL_ELEMENT1_NAME = "This is first element.";
        private const string OBJECT_FILL_ELEMENT2_ID = "3351";
        private const string OBJECT_FILL_ELEMENT2_NAME = "John Doe";

        private const int OBJECT_SET_ELEMENT1_ID = 500;
        private const string OBJECT_SET_ELEMENT1_NAME = "This is DE-MA-CIA";
        private const int OBJECT_SET_ELEMENT2_ID = 2000;
        private const string OBJECT_SET_ELEMENT2_NAME = "Today is a good day to eat.";
        #endregion 

        #region --OBJECTS--
        [DbAutoFill]
        public class ObjectWithStructured
        {
            public GenericSqlStructuredType MyStructuredField { get; set; }
            public MyGenericObject FindGenericObjectFromId(int id)
            {
                foreach(var element in MyStructuredField.Records)
                {
                    if (element.id == id)
                        return element;
                }

                return null;
            }
        }
        #endregion

        private SqlCommand _command;
        private ObjectWithStructured _dvco;
        private IDataReader _dataReader;

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void FillObjectFromDataReader_StructuredGood_Test()
        {
            DbAutoFillHelper.FillObjectFromDataReader(_dvco, _dataReader);

            Assert.IsTrue(_dvco.MyStructuredField.Records.Count > 0);
            MyGenericObject mgo = _dvco.FindGenericObjectFromId(int.Parse(OBJECT_FILL_ELEMENT1_ID));
            Assert.IsNotNull(mgo);

            Assert.AreEqual(mgo.name, OBJECT_FILL_ELEMENT1_NAME);

            mgo = _dvco.FindGenericObjectFromId(int.Parse(OBJECT_FILL_ELEMENT2_ID));
            Assert.IsNotNull(mgo);

            Assert.AreEqual(mgo.name, OBJECT_FILL_ELEMENT2_NAME);
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void SetParametersFromObject_StructuredGood_NotNull_Test()
        {
            _dvco.MyStructuredField = new GenericSqlStructuredType();
            _dvco.MyStructuredField.Add(new MyGenericObject() { id = OBJECT_SET_ELEMENT1_ID, name = OBJECT_SET_ELEMENT1_NAME });
            _dvco.MyStructuredField.Add(new MyGenericObject() { id = OBJECT_SET_ELEMENT2_ID, name = OBJECT_SET_ELEMENT2_NAME });
            DbAutoFillHelper.AddParametersFromObjectMembers(_command, _dvco);

            Assert.IsTrue(_command.Parameters.Contains(OBJECT_MYSTRUCTUREDFIELD_NAME));
            Assert.IsTrue(_command.Parameters[OBJECT_MYSTRUCTUREDFIELD_NAME].SqlDbType == SqlDbType.Structured);
        }

        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void SetParametersFromObject_StructuredGood_Null_Test()
        {
            DbAutoFillHelper.AddParametersFromObjectMembers(_command, _dvco);

            Assert.AreEqual(_command.Parameters.Count, 1);
            Assert.IsTrue(((INullable)_command.Parameters[OBJECT_MYSTRUCTUREDFIELD_NAME].SqlValue).IsNull);
        }

        [TestInitialize]
        public void Setup()
        {
            _command = new SqlCommand();
            _dvco = new ObjectWithStructured();
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
            DataTable dt = new DataTable("ObjectWithStructuredResultSets");
            dt.Columns.Add(OBJECT_MYSTRUCTUREDFIELD_NAME, typeof(string));
            dt.AcceptChanges();
            return dt;
        }

        private IDataReader CreateDataReader()
        {
            DataTable dt = CreateNewDataTable();
            dt.Rows.Add(string.Format("{0}|{1};{2}|{3};", OBJECT_FILL_ELEMENT1_ID, OBJECT_FILL_ELEMENT1_NAME, OBJECT_FILL_ELEMENT2_ID, OBJECT_FILL_ELEMENT2_NAME));
            dt.AcceptChanges();
            return dt.CreateDataReader();
        }
    }
}
