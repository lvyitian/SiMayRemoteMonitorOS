using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SiMay.ReflectCache
{
    /// <summary>
    /// 准备应用于反射缓存。。。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicMethodMemberAccessor : IMemberAccessor
    {
        private static Dictionary<Type, IMemberAccessor> classAccessors = new Dictionary<Type, IMemberAccessor>();

        public object GetValue(object instance, string memberName)
        {
            return FindClassAccessor(instance).GetValue(instance, memberName);
        }

        public void SetValue(object instance, string memberName, object newValue)
        {
            FindClassAccessor(instance).SetValue(instance, memberName, newValue);
        }

        private IMemberAccessor FindClassAccessor(object instance)
        {
            var typekey = instance.GetType();
            IMemberAccessor classAccessor;
            classAccessors.TryGetValue(typekey, out classAccessor);
            if (classAccessor == null)
            {
                classAccessor = Activator.CreateInstance(typeof(DynamicMethod<>).MakeGenericType(instance.GetType())) as IMemberAccessor;
                classAccessors.Add(typekey, classAccessor);
            }

            return classAccessor;
        }
    }

    public class DynamicMethod<T> : IMemberAccessor
    {
        internal static Func<object, string, object> GetValueDelegate;
        internal static Action<object, string, object> SetValueDelegate;

        public object GetValue(T instance, string memberName)
        {
            return GetValueDelegate(instance, memberName);
        }

        public void SetValue(T instance, string memberName, object newValue)
        {
            SetValueDelegate(instance, memberName, newValue);
        }

        public object GetValue(object instance, string memberName)
        {
            return GetValueDelegate(instance, memberName);
        }

        public void SetValue(object instance, string memberName, object newValue)
        {
            SetValueDelegate(instance, memberName, newValue);
        }

        static DynamicMethod()
        {
            GetValueDelegate = GenerateGetValue();
            SetValueDelegate = GenerateSetValue();
        }

        private static Func<object, string, object> GenerateGetValue()
        {
            var type = typeof(T);
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in type.GetProperties())
            {
                var property = Expression.Property(Expression.Convert(instance, typeof(T)), propertyInfo.Name);
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                cases.Add(Expression.SwitchCase(Expression.Convert(property, typeof(object)), propertyHash));
            }
            var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
            var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

            return Expression.Lambda<Func<object, string, object>>(methodBody, instance, memberName).Compile();
        }

        private static Action<object, string, object> GenerateSetValue()
        {
            var type = typeof(T);
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var newValue = Expression.Parameter(typeof(object), "newValue");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in type.GetProperties())
            {
                var property = Expression.Property(Expression.Convert(instance, typeof(T)), propertyInfo.Name);
                var setValue = Expression.Assign(property, Expression.Convert(newValue, propertyInfo.PropertyType));
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                cases.Add(Expression.SwitchCase(Expression.Convert(setValue, typeof(object)), propertyHash));
            }
            var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
            var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

            return Expression.Lambda<Action<object, string, object>>(methodBody, instance, memberName, newValue).Compile();
        }
    }
}
