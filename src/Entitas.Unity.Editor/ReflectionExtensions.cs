using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Entitas.Unity.Editor
{
    /// A public instance field or a public instance property with a getter and a setter.
    sealed class PublicMemberInfo
    {
        public readonly Type Type;
        public readonly string Name;

        readonly FieldInfo _fieldInfo;
        readonly PropertyInfo _propertyInfo;

        public PublicMemberInfo(FieldInfo info)
        {
            _fieldInfo = info;
            Type = info.FieldType;
            Name = info.Name;
        }

        public PublicMemberInfo(PropertyInfo info)
        {
            _propertyInfo = info;
            Type = info.PropertyType;
            Name = info.Name;
        }

        public object GetValue(object obj) => _fieldInfo != null
            ? _fieldInfo.GetValue(obj)
            : _propertyInfo.GetValue(obj, null);

        public void SetValue(object obj, object value)
        {
            if (_fieldInfo != null)
                _fieldInfo.SetValue(obj, value);
            else
                _propertyInfo.SetValue(obj, value, null);
        }
    }

    static class ReflectionExtensions
    {
        const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        static readonly Dictionary<string, string> BuiltInTypes = new Dictionary<string, string>
        {
            { "System.Boolean", "bool" },
            { "System.Byte", "byte" },
            { "System.SByte", "sbyte" },
            { "System.Char", "char" },
            { "System.Decimal", "decimal" },
            { "System.Double", "double" },
            { "System.Single", "float" },
            { "System.Int32", "int" },
            { "System.UInt32", "uint" },
            { "System.Int64", "long" },
            { "System.UInt64", "ulong" },
            { "System.Object", "object" },
            { "System.Int16", "short" },
            { "System.UInt16", "ushort" },
            { "System.String", "string" },
            { "System.Void", "void" }
        };

        public static IEnumerable<Type> GetAllTypes(this AppDomain appDomain)
        {
            var types = new List<Type>();
            foreach (var assembly in appDomain.GetAssemblies())
            {
                try
                {
                    types.AddRange(assembly.GetTypes());
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types.AddRange(ex.Types.Where(type => type != null));
                }
            }

            return types;
        }

        /// Creates an instance of every non abstract type that implements T and has a default constructor.
        public static IEnumerable<T> GetInstancesOf<T>(this AppDomain appDomain) => appDomain
            .GetAllTypes()
            .Where(type => !type.IsAbstract
                           && !type.IsGenericTypeDefinition
                           && typeof(T).IsAssignableFrom(type)
                           && type.GetConstructor(Type.EmptyTypes) != null)
            .Select(type => (T)Activator.CreateInstance(type));

        public static bool ImplementsInterface<T>(this Type type) =>
            !type.IsInterface && type.GetInterface(typeof(T).FullName) != null;

        public static PublicMemberInfo[] GetPublicMemberInfos(this Type type)
        {
            var fields = type.GetFields(PublicInstance)
                .Select(info => new PublicMemberInfo(info));

            var properties = type.GetProperties(PublicInstance)
                .Where(info => info.CanRead && info.CanWrite && info.GetIndexParameters().Length == 0)
                .Select(info => new PublicMemberInfo(info));

            return fields.Concat(properties).ToArray();
        }

        public static void CopyPublicMemberValues(this IComponent source, object target)
        {
            foreach (var info in source.GetType().GetPublicMemberInfos())
                info.SetValue(target, info.GetValue(source));
        }

        /// The type name as it is written in C# code, e.g. List&lt;int&gt;, int[,] or Outer.Inner.
        public static string ToCompilableString(this Type type)
        {
            if (BuiltInTypes.TryGetValue(type.FullName, out var builtIn))
                return builtIn;

            if (type.IsGenericType)
            {
                var arguments = type.GetGenericArguments().Select(ToCompilableString);
                var name = type.FullName.Split('`')[0];
                return name + "<" + string.Join(", ", arguments) + ">";
            }

            if (type.IsArray)
                return type.GetElementType().ToCompilableString() + "[" + new string(',', type.GetArrayRank() - 1) + "]";

            if (type.IsNested)
                return type.FullName.Replace('+', '.');

            return type.FullName;
        }

        /// The part after the last dot, e.g. "System.String" becomes "String".
        public static string TypeName(this string fullTypeName)
        {
            var index = fullTypeName.LastIndexOf(".", StringComparison.Ordinal) + 1;
            return fullTypeName.Substring(index, fullTypeName.Length - index);
        }
    }
}
