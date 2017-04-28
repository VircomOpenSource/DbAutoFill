using System;
using System.Collections.Generic;

namespace DatabaseAutoFill.Types
{
    public class DbResponse<T>
    {
        public bool HasResult
        {
            get { return ResultSet.Count > 0; }
        }

        public bool HasError
        {
            get { return Exception != null; }
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public Exception Exception
        {
            get;
            private set;
        }

        public IList<T> ResultSet
        {
            get;
            private set;
        }

        public DbResponse()
        {
            ResultSet = new List<T>();
        }

        public DbResponse(string errorMessage, Exception ex)
            : this()
        {
            if (ex == null)
                throw new ArgumentNullException("ex");
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentException("errorMessage cannot be empty.", "errorMessage");

            Exception = ex;
            ErrorMessage = errorMessage;
        }

        public void Add(T element)
        {
            if (element == null)
                throw new ArgumentNullException("element", "Cannot add null element.");

            ResultSet.Add(element);
        }
    }
}
