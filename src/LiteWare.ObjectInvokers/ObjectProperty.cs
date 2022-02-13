using System.Reflection;

namespace LiteWare.ObjectInvokers;

/// <summary>
/// Represents an object's property.
/// </summary>
public class ObjectProperty : ObjectMember<PropertyInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectProperty"/> class with the provided property info.
    /// </summary>
    /// <param name="propertyInfo">Information about the property.</param>
    /// <param name="preferredName">The preferred property name. Defaults to the original property name if not provided.</param>
    public ObjectProperty(PropertyInfo propertyInfo, string? preferredName = null)
        : base(propertyInfo, preferredName) { }

    /// <summary>
    /// Gets a value indicating whether the object's property can be read.
    /// </summary>
    public bool CanRead => TargetMemberInfo.CanRead;

    /// <summary>
    /// Gets a value indicating whether the object's property can be written to.
    /// </summary>
    public bool CanWrite => TargetMemberInfo.CanWrite;

    /// <summary>
    /// Builds the member's signature.
    /// </summary>
    /// <param name="propertyInfo">Information about the property.</param>
    /// <returns>The signature of the member.</returns>
    protected override MemberSignature BuildSignature(PropertyInfo propertyInfo)
    {
        List<MemberParameter> parameters = new();
        if (propertyInfo.CanWrite)
        {
            MemberParameter parameter = new(propertyInfo.PropertyType, propertyInfo.CanRead, false);
            parameters.Add(parameter);
        }

        return new MemberSignature(propertyInfo.Name, 0, parameters.ToArray());
    }

    /// <summary>
    /// Returns the property value from the provided <paramref name="objectInstance"/>.
    /// </summary>
    /// <param name="objectInstance">The object instance whose property value will be returned.</param>
    /// <returns>The property value of the provided object.</returns>
    /// <exception cref="InvalidOperationException">The object's property cannot be read.</exception>
    public object? GetValue(object objectInstance)
    {
        if (!CanRead)
        {
            throw new InvalidOperationException("Property is write only.");
        }

        return TargetMemberInfo.GetValue(objectInstance);
    }

    /// <summary>
    /// Sets the property value of the provided <paramref name="objectInstance"/>.
    /// </summary>
    /// <param name="objectInstance">The object instance whose property value will be set.</param>
    /// <param name="value">The new property value.</param>
    /// <exception cref="InvalidOperationException">The object's property cannot be written to.</exception>
    public void SetValue(object objectInstance, object? value)
    {
        if (!CanWrite)
        {
            throw new InvalidOperationException("Property is read only.");
        }

        TargetMemberInfo.SetValue(objectInstance, value);
    }

    /// <summary>
    /// Invokes the underlying property represented by this class with the provided generic types and parameters.
    /// </summary>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="genericTypes">Any generic types used in the invoke process.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public override object? Invoke(object objectInstance, Type[]? genericTypes, object?[]? parameters)
    {
        if (genericTypes != null && genericTypes.Any())
        {
            throw new ArgumentException("Property invoke cannot accept generic types.");
        }

        if (parameters is null || parameters.Length == 0)
        {
            return GetValue(objectInstance);
        }

        if (parameters.Length == 1)
        {
            SetValue(objectInstance, parameters.First());
            return null;
        }

        throw new ArgumentException("Setter invoke should contain only 1 parameter.");
    }
}