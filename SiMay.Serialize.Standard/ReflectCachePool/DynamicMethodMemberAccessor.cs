using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SiMay.ReflectCache
{
    public class DynamicMethodMemberAccessor
    {
        private static Dictionary<Type, IMemberAccessor> classAccessors = new Dictionary<Type, IMemberAccessor>();

        static DynamicMethodMemberAccessor()
        {
            var currentDomainTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(c => c.GetTypes());
            foreach (var type in currentDomainTypes.Where(c => c.IsSubclassOf(typeof(EntitySerializerBase))))
                classAccessors.Add(type, CreateMemberAccessor(type));

        }
        public static IMemberAccessor FindClassAccessor(Type instanceType)
        {
            IMemberAccessor classAccessor;
            if (!classAccessors.TryGetValue(instanceType, out classAccessor))
                classAccessors.Add(instanceType, CreateMemberAccessor(instanceType));

            return classAccessor;
        }

        private static IMemberAccessor CreateMemberAccessor(Type type)
        {
            var instance = Activator.CreateInstance(typeof(DynamicMethod<>).MakeGenericType(type)) as IMemberAccessor;
            if (instance == null)
                throw new Exception("Activator.CreateInstance Object is empty");
            return instance;
        }
    }

    public class DynamicMethod<T> : IMemberAccessor
    {
        internal static Func<object, string, object> GetValueDelegate;
        internal static Action<object, string, object> SetValueDelegate;

        public Type Type { get; set; } = typeof(T);

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

            //创建int nameHash = instance.GetHashCode();表达式
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
