using System;

namespace DatabaseAutoFill.Types
{
    public abstract class IDbAnonymousValue
    {
        /// <summary>
        /// Alias to which the AnonymousValue is bound to (column in result set or parameter name).
        /// </summary>
        public string Alias { get; set; }

        protected object _value;

        /// <summary>
        /// </summary>
        /// <returns>Type object of the inner value.</returns>
        public Type GetValueType()
        {
            if (_value == null)
                return typeof(DBNull);

            return _value.GetType();
        }

        public abstract object GetValue();
        public abstract void SetValue(object value);
    }

    public class DbAnonymousValue<T> : IDbAnonymousValue
    {
        public DbAnonymousValue()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="alias">Non-empty alias name (used as parameter name or column name in result set).</param>
        /// <param name="value"></param>
        public DbAnonymousValue(string alias, T value)
        {
            Alias = alias;
            _value = value;
        }

        public override object GetValue()
        {
            return GetTypedValue();
        }

        /// <summary>
        /// </summary>
        /// <param name="value">Value must be of T type.</param>
        /// <exception cref="ArgumentException"></exception>
        public override void SetValue(object value)
        {
            if (value.GetType() != typeof(T))
                throw new ArgumentException("Value is not of the right type. Type given:" + value.GetType().Name + " instead of " + typeof(T).Name);

            _value = value;
        }

        public T GetTypedValue()
        {
            return (T)_value;
        }
    }
}
