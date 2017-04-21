using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseAutoFill.Types
{
    internal interface IDbStructuredType
    {
        void SetParameterValue(IDbDataParameter parameter);
        void FromSerializedString(string serialized);
        bool IsEmpty();
    }

    /// <summary>
    /// Represents structured data type (table-valued type).
    /// </summary>
    /// <typeparam name="TData">Inner CLR datatype</typeparam>
    /// <typeparam name="TDbDataRecord"></typeparam>
    public abstract class DbStructuredType<TData, TDbDataRecord> : IEnumerable<TDbDataRecord>, IDbStructuredType
        where TDbDataRecord : IDataRecord
    {
        public abstract IEnumerator<TDbDataRecord> GetEnumerator();

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FormatException"></exception>
        public abstract void FromSerializedString(string serialized);

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public abstract void SetParameterValue(IDbDataParameter parameter);

        private IList<TData> _records;

        public IList<TData> Records { get { return _records; } }

        public DbStructuredType()
        {
            _records = new List<TData>();
        }

        /// <exception cref="NotSupportedException">From IList.Add()</exception>
        public virtual void Add(TData value)
        {
            _records.Add(value);
        }

        public bool IsEmpty()
        {
            return _records.Count == 0;
        }

        public int Count()
        {
            return _records.Count;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <exception cref="NotSupportedException">From IList.Clear()</exception>
        public void Clear()
        {
            _records.Clear();
        }

        public bool Contains(TData item)
        {
            return _records.Contains(item);
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void CopyTo(TData[] array, int arrayIndex)
        {
            _records.CopyTo(array, arrayIndex);
        }

        /// <exception cref="NotSupportedException">From ICollection.Remove</exception>
        public bool Remove(TData item)
        {
            return _records.Remove(item);
        }
    }
}
