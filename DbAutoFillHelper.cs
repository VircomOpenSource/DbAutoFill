using DatabaseAutoFill.Types;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DatabaseAutoFill
{
    public static class DbAutoFillHelper
    {
        /// <summary>
        /// Add a parameter to the command's parameters collection.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException">Param mustn't be null.</exception>
        public static void AddParameterWithValue<T>(IDbCommand command, DbAnonymousValue<T> param)
        {
            if (param == null)
                throw new ArgumentNullException("param", "A non-null DbAnonymousValue must be provided.");

            AddParameterWithValue<T>(command, param.Alias, param.GetTypedValue(), null);
        }

        /// <summary>
        /// Add a parameter to the command's parameters collection.
        /// 
        /// Supports null value.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddParameterWithValue<T>(IDbCommand command, string name, T value, DbType? dbType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Parameter name cannot be empty.", "name");
            if (command == null)
                throw new ArgumentNullException("command");

            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;

            IDbStructuredType structuredValue = value as IDbStructuredType;

            if (structuredValue != null)
            {
                if (structuredValue.IsEmpty())
                    return;

                structuredValue.SetParameterValue(parameter);
            }
            else
            {
                parameter.Value = value;

                if (dbType != null && dbType.HasValue)
                    parameter.DbType = dbType.Value;

                if (value == null)
                    parameter.Value = DBNull.Value;
            }

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Create and assign the parameters to the given DbCommand object.
        /// Parameters created from the names and values from the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="obj"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddParametersFromObjectMembers<T>(IDbCommand command, T obj)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type modelType = obj.GetType();

            DbAutoFillAttribute modelAttribute = modelType
                .GetCustomAttributes(typeof(DbAutoFillAttribute), true)
                .FirstOrDefault() as DbAutoFillAttribute;

            if (modelAttribute != null)
            {
                if (modelAttribute.FillBehavior == FillBehavior.None || modelAttribute.FillBehavior == FillBehavior.FromDB)
                    return;
            }

            PropertyInfo[] modelPropertyInfos = modelType.GetProperties();
            FieldInfo[] modelFieldInfos = modelType.GetFields();

            string modelParameterPrefix = modelAttribute.ParameterPrefix ?? "";

            AddParametersFromMemberInfos(command, obj, modelPropertyInfos, modelAttribute, modelParameterPrefix);
            AddParametersFromMemberInfos(command, obj, modelFieldInfos, modelAttribute, modelParameterPrefix);
        }

        /// <summary>
        /// Fills the model fromthe result set / data reader. 
        /// It uses the DbAutoFillAttribute to fill the object.
        /// 
        /// After a call to this method, the model should contain the data properly.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TypeLoadException"></exception>
        /// <exception cref="MissingFieldException"></exception>
        public static void FillObjectFromDataReader<T>(T obj, IDataReader dataReader)
            where T : new()
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (dataReader == null)
                throw new ArgumentNullException("dataReader");

            Type modelType = obj.GetType();

            if (modelType.IsSubclassOf(typeof(IDbAnonymousValue)))
            {
                SetDbAnonymousValueFromDataReader(obj as IDbAnonymousValue, dataReader);
                return;
            }

            DbAutoFillAttribute modelAttribute = Attribute.GetCustomAttribute(modelType, typeof(DbAutoFillAttribute)) as DbAutoFillAttribute;

            if (modelAttribute != null)
            {
                if (modelAttribute.FillBehavior == FillBehavior.None || modelAttribute.FillBehavior == FillBehavior.ToDB)
                    return;
            }

            PropertyInfo[] modelProperties = modelType.GetProperties();
            FieldInfo[] modelFields = modelType.GetFields();

            string[] lstDbFields = GetFieldsFromDataReader(dataReader);

            SetMembersValuesFromDataReader(dataReader, obj, modelProperties, lstDbFields, modelAttribute);
            SetMembersValuesFromDataReader(dataReader, obj, modelFields, lstDbFields, modelAttribute);
        }
        
        private static void AddParametersFromMemberInfos<T>(IDbCommand command, T obj, MemberInfo[] memberInfos, DbAutoFillAttribute modelAttribute, string modelParameterPrefix)
        {
            foreach (var mi in memberInfos)
            {
                DbAutoFillAttribute memberAttribute = mi.GetCustomAttributes(typeof(DbAutoFillAttribute), false)
                    .FirstOrDefault() as DbAutoFillAttribute;

                if (memberAttribute == null && modelAttribute == null)
                    continue;

                AddParameterFromAttribute(command, obj, modelParameterPrefix, mi.Name, memberAttribute);
            }
        }

        private static void SetMembersValuesFromDataReader<T>(IDataReader reader, T obj, MemberInfo[] memberInfos, string[] lstDbFields, DbAutoFillAttribute modelAttribute)
        {
            foreach (var mi in memberInfos)
            {
                DbAutoFillAttribute memberAttribute = mi.GetCustomAttributes(typeof(DbAutoFillAttribute), false)
                    .FirstOrDefault() as DbAutoFillAttribute;

                if (memberAttribute == null && modelAttribute == null)
                    continue;

                SetValueForObjectMember(reader, obj, mi, memberAttribute, mi.Name, lstDbFields);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="MissingFieldException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static void SetValueForObjectMember<T>(IDataReader reader, T obj, MemberInfo memberInfo, DbAutoFillAttribute attribute, string fieldName, string[] lstDbFields)
        {
            string columnName = fieldName;

            if (attribute != null)
            {
                if (attribute.FillBehavior == FillBehavior.None || attribute.FillBehavior == FillBehavior.ToDB)
                    return;

                columnName = attribute.Alias ?? fieldName;
            }

            bool hasFieldInReader = lstDbFields.Contains(columnName);

            if (!hasFieldInReader)
            {
                if (attribute != null && attribute.AllowMissing)
                    return;

                throw new MissingFieldException(string.Format("No column named '{0}' in reader for object '{1}'.", columnName, obj.GetType().FullName));
            }

            object value;

            if (reader.IsDBNull(reader.GetOrdinal(columnName)))
                value = null;

            if (memberInfo is PropertyInfo)
            {
                PropertyInfo pi = memberInfo as PropertyInfo;

                value = GetValueFromColumn(pi.PropertyType, reader[columnName]);

                pi.SetValue(obj, value, null);
            }
            else if (memberInfo is FieldInfo)
            {
                FieldInfo fi = memberInfo as FieldInfo;

                value = GetValueFromColumn(fi.FieldType, reader[columnName]);

                fi.SetValue(obj, value);
            }
            else
                throw new ArgumentException("memberInfo", "Unsupported field type");

        }

        private static object GetValueFromColumn(Type typeOfField, object dataReaderContent)
        {
            if (dataReaderContent == DBNull.Value)
                return null;

            object value = null;

            if (typeOfField == typeof(Guid) || typeOfField == typeof(Guid?))
            {
                value = new Guid(dataReaderContent.ToString());
            }
            else if (typeof(IDbStructuredType).IsAssignableFrom(typeOfField))
            {
                value = Activator.CreateInstance(typeOfField);
                (value as IDbStructuredType).FromSerializedString(dataReaderContent.ToString());
            }
            else if (typeOfField.IsGenericType && typeOfField.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                value = Convert.ChangeType(dataReaderContent, Nullable.GetUnderlyingType(typeOfField));
            }
            else
                value = Convert.ChangeType(dataReaderContent, typeOfField);

            return value;
        }

        private static void SetDbAnonymousValueFromDataReader(IDbAnonymousValue anonymousValue, IDataReader dr)
        {
            anonymousValue.SetValue(Convert.ChangeType(dr[0], anonymousValue.GetType().GetGenericArguments()[0]));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>false if cannot get filled</returns>
        private static bool ParseAttributeInfosForParameter(DbAutoFillAttribute attribute, string modelParameterPrefix, string fieldName, ref string parameterName, ref DbType? sqlType)
        {
            if (attribute == null)
                return true;

            if (attribute.FillBehavior == FillBehavior.None || attribute.FillBehavior == FillBehavior.FromDB)
                return false;

            string propertyPrefix = modelParameterPrefix;

            if (!string.IsNullOrWhiteSpace(attribute.ParameterPrefix))
                propertyPrefix = attribute.ParameterPrefix;

            string specifiedName = attribute.Alias;

            sqlType = attribute.DbType;

            if (!string.IsNullOrWhiteSpace(specifiedName))
                parameterName = propertyPrefix + specifiedName;
            else
                parameterName = propertyPrefix + fieldName;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private static void AddParameterFromAttribute<T>(IDbCommand cmd, T model, string modelParameterPrefix, string propertyName, DbAutoFillAttribute propertyAttribute)
        {
            string parameterName = modelParameterPrefix + propertyName;
            DbType? sqlType = null;

            if (!ParseAttributeInfosForParameter(propertyAttribute, modelParameterPrefix, propertyName, ref parameterName, ref sqlType))
                return;

            object parameterValue = model.GetType()
                .GetProperty(propertyName)
                .GetValue(model, null);

            AddParameterWithValue(cmd, parameterName, parameterValue, sqlType);
        }

        /// <summary>
        /// 
        /// </summary>
        private static string[] GetFieldsFromDataReader(IDataReader reader)
        {
            string[] lstFields = new string[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; ++i)
                lstFields[i] = reader.GetName(i);

            return lstFields;
        }
    }
}
