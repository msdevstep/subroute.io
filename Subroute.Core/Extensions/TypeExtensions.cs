using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Subroute.Core.Extensions
{
    public static class TypeExtensions
    {
        public static T CreateInstance<T>(this Type type, params object[] args)
        {
            return (T)CreateInstance(type, args);
        }

        public static object CreateInstance(this Type type, params object[] args)
        {
            var constructor = type.GetConstructor(args.Select(a => a.GetType()).ToArray());

            if (constructor == null)
                throw new NullReferenceException("No constructor was found for '(type.Name)' with the passed parameters.");

            var argExpressions = args.Select(Expression.Constant).Cast<Expression>().ToArray();
            return Expression
                .Lambda(Expression.New(constructor, argExpressions))
                .Compile()
                .DynamicInvoke();
        }

        public static object Coerce(this Type destinationType, object value, bool useDateTimeOffset = false)
        {
            if (value == null || value == DBNull.Value)
                return null;

            var sourceType = value.GetType();
            if (sourceType == destinationType)
            {
                if (sourceType == typeof(DateTime) && useDateTimeOffset)
                    return new DateTimeOffset((DateTime)value);

                return value;
            }

            var converter = TypeDescriptor.GetConverter(destinationType);
            return converter.ConvertFrom(value);
        }
    }
}