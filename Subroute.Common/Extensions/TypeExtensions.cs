using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Subroute.Common.Extensions
{
    public static class TypeExtensions
    {
        public static object CreateInstance(this Type type, params object[] args)
        {
            var constructor = type.GetConstructor(args.Select(a => a.GetType()).ToArray());

            if (constructor == null)
                throw new NullReferenceException("No constructor was found for '" + type.Name + "' with the passed parameters.");

            var argExpressions = args.Select(Expression.Constant).Cast<Expression>().ToArray();
            return Expression
                .Lambda(Expression.New(constructor, argExpressions))
                .Compile()
                .DynamicInvoke();
        }

        public static bool InheritsFrom(this Type type, Type baseType)
        {
            // null does not have base type
            if (type == null)
            {
                return false;
            }

            // only interface can have null base type
            if (baseType == null)
            {
                return type.IsInterface;
            }

            // check implemented interfaces
            if (baseType.IsInterface)
            {
                return type.GetInterfaces().Contains(baseType);
            }

            // check all base types
            var currentType = type;
            while (currentType != null)
            {
                if (currentType.BaseType == baseType)
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }

        public static object Coerce(this Type destinationType, object value)
        {
            if (value == null)
                return null;

            var sourceType = value.GetType();
            var innerValue = value;

            // Handle special case of string to DateTime.
            if (destinationType == typeof(DateTime) && sourceType == typeof(String))
                return DateTime.Parse((string)value);

            var converter = TypeDescriptor.GetConverter(destinationType);
            return converter.ConvertTo(innerValue, destinationType);
        }

        public static bool IsNullable(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsPrimitive(this Type type)
        {
            return (type.IsValueType || type.IsNullable() || type == typeof(string));
        }
    }
}