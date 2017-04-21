using System;
using System.Data;

namespace DatabaseAutoFill
{
    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Field
        | AttributeTargets.Property
    )]
    public class DbAutoFillAttribute : System.Attribute
    {
        /// <summary>
        /// Alias to the column's name in the DataReader or the StoredProcedure parameter.
        /// If not set (null or empty), it will use the field's name (or property's name) instead.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// When in FillMode
        /// </summary>
        public string ParameterPrefix { get; set; }

        /// <summary>
        /// Allow specifying the DbType for the current property or field (e.g. DateTime2 instead of DateTime).
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// Determines the fill behavior of the object, property or field.
        /// Default is FillBehavior.Both
        /// </summary>
        public FillBehavior FillBehavior { get; set; }

        /// <summary>
        /// Defines if the property or the field is allowed to be missing from the DataReader. 
        /// </summary>
        public bool AllowMissing { get; set; }

        public DbAutoFillAttribute()
        {
            AllowMissing = false;
            FillBehavior = FillBehavior.Both;
        }
    }

    public enum FillBehavior
    {
        /// <summary>
        /// Fills the object from the database result only.
        /// </summary>
        FromDB,

        /// <summary>
        /// Uses the object value(s) to send to the database as parameters.
        /// </summary>
        ToDB,

        /// <summary>
        /// Combines FromDB and ToDB. 
        /// </summary>
        Both,

        /// <summary>
        /// Have no behavior. Doesn't communicate with DB.
        /// </summary>
        None
    }
}
