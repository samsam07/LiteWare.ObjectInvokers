using System.Reflection;

#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LiteWare.ObjectInvokers.UnitTests")]
#endif
namespace LiteWare.ObjectInvokers.Utilities;

// Adapted from https://stackoverflow.com/questions/32025201/how-can-i-determine-if-an-implicit-cast-exists-in-c
internal static class TypeUtility
{
    private static readonly Dictionary<Type, List<Type>> ImplicitNumericConversions = new();

    static TypeUtility()
    {
        ImplicitNumericConversions.Add(typeof(sbyte), new List<Type> { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) });
        ImplicitNumericConversions.Add(typeof(byte), new List<Type> { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) });
        ImplicitNumericConversions.Add(typeof(short), new List<Type> { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) });
        ImplicitNumericConversions.Add(typeof(ushort), new List<Type> { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) });
        ImplicitNumericConversions.Add(typeof(int), new List<Type> { typeof(long), typeof(float), typeof(double), typeof(decimal) });
        ImplicitNumericConversions.Add(typeof(uint), new List<Type> { typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) });
        ImplicitNumericConversions.Add(typeof(long), new List<Type> { typeof(float), typeof(double), typeof(decimal) });
        ImplicitNumericConversions.Add(typeof(char), new List<Type> { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) });
        ImplicitNumericConversions.Add(typeof(float), new List<Type> { typeof(double) });
        ImplicitNumericConversions.Add(typeof(ulong), new List<Type> { typeof(float), typeof(double), typeof(decimal) });
    }

    public static bool IsNullable(Type type) =>
        (!type.IsValueType || Nullable.GetUnderlyingType(type) is not null);

    public static bool CanImplicitlyConvert(Type typeFrom, Type typeTo)
    {
        if (typeTo == typeof(object))
        {
            return true;
        }

        if (typeTo.IsAssignableFrom(typeFrom))
        {
            return true;
        }

        if (HasImplicitConversion(typeFrom, typeFrom, typeTo) || HasImplicitConversion(typeTo, typeFrom, typeTo))
        {
            return true;
        }

        if (ImplicitNumericConversions.TryGetValue(typeFrom, out List<Type>? list) && list.Contains(typeTo))
        {
            return true;
        }

        Type? underlyingNullableType = Nullable.GetUnderlyingType(typeTo);
        if (underlyingNullableType is not null)
        {
            return CanImplicitlyConvert(typeFrom, underlyingNullableType);
        }

        return false;
    }

    private static bool HasImplicitConversion(Type definedOn, Type baseType, Type targetType)
    {
        bool hasImplicitConversion = definedOn
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "op_Implicit" && m.ReturnType == targetType)
            .Any(m => m.GetParameters().FirstOrDefault()?.ParameterType == baseType);

        return hasImplicitConversion;
    }
}