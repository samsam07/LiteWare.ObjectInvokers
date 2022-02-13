using System.Reflection;
using LiteWare.ObjectInvokers.Attributes;

namespace LiteWare.ObjectInvokers.Extensions;

/// <summary>
/// Provide extension methods for object member extraction.
/// </summary>
public static class ObjectContractExtension
{
    /// <summary>
    /// Represents the supported binding flags used in object member extraction through reflection.
    /// </summary>
    public const BindingFlags SupportedMemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static;

    /// <summary>
    /// Finds all the invokable members marked with <see cref="InvokableMemberAttribute"/> from <paramref name="contractType"/>.
    /// </summary>
    /// <param name="contractType">The type on which members marked by the <see cref="InvokableMemberAttribute"/> will be extracted.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains invokable object members marked with <see cref="InvokableMemberAttribute"/>.</returns>
    /// <exception cref="NotSupportedException">The <paramref name="contractType"/> contains unsupported members marked with <see cref="InvokableMemberAttribute"/>.</exception>
    public static IEnumerable<IObjectMember> FindInvokableMembers(this Type contractType)
    {
        MemberInfo[] contractMembers = contractType.GetMembers(SupportedMemberBindingFlags);
        foreach (MemberInfo contractMember in contractMembers)
        {
            InvokableMemberAttribute? invokableMemberAttribute = contractMember.GetCustomAttribute<InvokableMemberAttribute>(true);
            if (invokableMemberAttribute is not null)
            {
                yield return contractMember.ToObjectMember(invokableMemberAttribute.PreferredName);
            }
        }
    }

    /// <summary>
    /// Converts a member info into an instance of <see cref="IObjectMember"/>.
    /// </summary>
    /// <param name="contractMemberInfo">The member info.</param>
    /// <param name="preferredName">The preferred object member name.</param>
    /// <returns>An instance of <see cref="IObjectMember"/>.</returns>
    /// <exception cref="NotSupportedException">The <paramref name="contractMemberInfo"/> member is not supported and cannot be converted to an instance of <see cref="IObjectMember"/>.</exception>
    public static IObjectMember ToObjectMember(this MemberInfo contractMemberInfo, string? preferredName = null) =>
        contractMemberInfo switch
        {
            MethodInfo methodInfo => new ObjectMethod(methodInfo, preferredName),
            PropertyInfo propertyInfo => new ObjectProperty(propertyInfo, preferredName),
            FieldInfo fieldInfo => new ObjectField(fieldInfo, preferredName),
            _ => throw new NotSupportedException($"Member of the type '{contractMemberInfo.GetType().Name}' is not supported.")
        };
}