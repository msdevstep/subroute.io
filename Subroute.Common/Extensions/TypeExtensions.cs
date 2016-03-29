using System;
using System.ComponentModel;
using System.IO;
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

    public static class StreamExtensions
    {
        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        public static byte[] ReadFully(this Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
                initialLength = 32768;

            var buffer = new byte[initialLength];
            var read = 0;
            int chunk;

            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    var nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    var newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }

            // Buffer is now too big. Shrink it.
            var ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}