using DatabaseAutoFill;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace test_DatabaseAutoFill
{
    [TestClass]
    public class DbCommandHelperTests
    {
        [TestMethod]
        [TestCategory("DatabaseAutoFill")]
        public void ValidateConnection_Test()
        {
            bool argEx = false;
            try
            {
                SqlCommandHelper sch = new SqlCommandHelper("");
            }
            catch (ArgumentException)
            {
                argEx = true;
            }

            Assert.IsTrue(argEx);
        }
    }
}
