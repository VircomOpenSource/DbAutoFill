using DatabaseAutoFill.Types;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;

namespace test_DatabaseAutoFill.Objects
{
    public class MyGenericObject
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class GenericSqlStructuredType : SqlStructuredType<MyGenericObject>
    {
        public override IEnumerator<SqlDataRecord> GetEnumerator()
        {
            foreach (MyGenericObject obj in Records)
            {
                SqlMetaData mdLocalPart = new SqlMetaData("MyId", SqlDbType.Int);
                SqlMetaData mdDomainPart = new SqlMetaData("MyName", SqlDbType.NVarChar, 50);

                SqlDataRecord record = new SqlDataRecord(mdLocalPart, mdDomainPart);
                record.SetSqlInt32(0, new SqlInt32(obj.id));
                record.SetSqlString(1, new SqlString(obj.name));

                yield return record;
            }
        }

        public override void FromSerializedString(string serialized)
        {
            if (serialized == null)
                return;

            string[] elements = serialized.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string element in elements)
            {
                string[] parts = element.Split('|');
                MyGenericObject mgo = new MyGenericObject();
                mgo.id = int.Parse(parts[0]);
                mgo.name = parts[1];
                Add(mgo);
            }
        }
    }
}
