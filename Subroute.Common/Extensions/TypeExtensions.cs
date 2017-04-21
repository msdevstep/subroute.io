using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Subroute.Common.Extensions
{
    /// <summary>
    /// Various extension methods for working with types.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Uses expression tress to create an instance of the specified type.
        /// </summary>
        /// <param name="type">Type of instance to create.</param>
        /// <param name="args">Arguments to be passed to type constructor.</param>
        /// <returns>Concrete instance of specified type.</returns>
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

        /// <summary>
        /// Indicates if the first type inherits from the base type.
        /// </summary>
        /// <param name="type">Type to check for inheritance of base type.</param>
        /// <param name="baseType">Base type to check for inheritance from.</param>
        /// <returns>Boolean value indicating whether the specified type inherits from the specified base type.</returns>
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

        /// <summary>
        /// Coerces one type to another type, even when there isn't a direct cast.
        /// </summary>
        /// <param name="destinationType">Final type to be returned.</param>
        /// <param name="value">Value to be coerced into the specified type.</param>
        /// <returns>Instance of destination type.</returns>
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

        /// <summary>
        /// Determines if a type is nullable.
        /// </summary>
        /// <param name="type">Type to check if nullable.</param>
        /// <returns>Boolean indicating if type is nullable.</returns>
        public static bool IsNullable(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Determines if a type is considered a primitive type (value type, nullable, or string).
        /// </summary>
        /// <param name="type">Type to be checked if primitive.</param>
        /// <returns>Boolean indicating if type is primitive.</returns>
        public static bool IsPrimitive(this Type type)
        {
            return (type.IsValueType || type.IsNullable() || type == typeof(string));
        }

        /// <summary>
        /// Determines if a method is marked with an async keyword.
        /// </summary>
        /// <param name="method">Method to check for async keyword.</param>
        /// <returns>Boolean indicating if method is async.</returns>
        public static bool IsAsyncMethod(this MethodInfo method)
        {
            // Obtain the method with the specified name.
            var attType = typeof(AsyncStateMachineAttribute);

            // Obtain the custom attribute for the method. 
            // The value returned contains the StateMachineType property. 
            // Null is returned if the attribute isn't present for the method. 
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(attType);

            return (attrib != null);
        }
    }
}