using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Subroute.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static Delegate GetPropertyGetter<T>(this T obj, string propertyName)
        {
            var elementType = obj.GetType();
            var pi = elementType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            var instance = Expression.Parameter(pi.DeclaringType, "i");
            var property = Expression.Property(instance, pi);
            var convert = Expression.TypeAs(property, typeof(object));

            return Expression.Lambda(convert, instance).Compile();
        }

        public static Delegate GetPropertySetter<T>(this T obj, string propertyName)
        {
            var elementType = obj.GetType();
            var pi = elementType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            var mi = pi.SetMethod;

            var instance = Expression.Parameter(pi.DeclaringType, "i");
            var argument = Expression.Parameter(typeof(object), "a");
            var setterCall = Expression.Call(instance, mi, Expression.Convert(argument, pi.PropertyType));

            return Expression.Lambda(setterCall, instance, argument).Compile();
        }

        public static bool SetProperty(this object instance, string propertyName, object value)
        {
            var property = instance?.GetType().GetProperty(propertyName);

            if (property == null)
                return false;

            var setter = instance.GetPropertySetter(propertyName);

            if (setter == null)
                return false;

            setter.DynamicInvoke(instance, property.PropertyType.Coerce(value));
            return true;
        }

        public static T GetProperty<T>(this object instance, string propertyName)
        {
            if (instance?.GetType().GetProperty(propertyName) == null)
                return default(T);

            var getter = instance.GetPropertyGetter(propertyName);

            if (getter == null)
                return default(T);

            return (T)getter.DynamicInvoke(instance);
        }
    }
}