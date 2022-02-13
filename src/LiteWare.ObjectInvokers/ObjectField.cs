using System.Reflection;

namespace LiteWare.ObjectInvokers;

/// <summary>
/// Represents an object's field.
/// </summary>
public class ObjectField : ObjectMember<FieldInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectField"/> class with the provided field info.
    /// </summary>
    /// <param name="fieldInfo">Information about the field.</param>
    /// <param name="preferredName">The preferred field name. Defaults to the original field name if not provided.</param>
    public ObjectField(FieldInfo fieldInfo, string? preferredName = null)
        : base(fieldInfo, preferredName) { }

    /// <summary>
    /// Gets a value indicating whether the object's field can be written to.
    /// </summary>
    public bool CanWrite => (TargetMemberInfo is { IsInitOnly: false, IsLiteral: false });

    /// <summary>
    /// Builds the member's signature.
    /// </summary>
    /// <param name="fieldInfo">Information about the field.</param>
    /// <returns>The signature of the member.</returns>
    protected override MemberSignature BuildSignature(FieldInfo fieldInfo)
    {
        List<MemberParameter> parameters = new();
        if (CanWrite)
        {
            MemberParameter parameter = new(fieldInfo.FieldType, true, false);
            parameters.Add(parameter);
        }

        return new MemberSignature(fieldInfo.Name, 0, parameters.ToArray());
    }

    /// <summary>
    /// Returns the field value from the provided <paramref name="objectInstance"/>.
    /// </summary>
    /// <param name="objectInstance">The object instance whose field value will be returned.</param>
    /// <returns>The field value of the provided object.</returns>
    public object? GetValue(object objectInstance) =>
        TargetMemberInfo.GetValue(objectInstance);

    /// <summary>
    /// Sets the field value of the provided <paramref name="objectInstance"/>.
    /// </summary>
    /// <param name="objectInstance">The object instance whose field value will be set.</param>
    /// <param name="value">The new field value.</param>
    /// <exception cref="InvalidOperationException">The object's field cannot be written to.</exception>
    public void SetValue(object objectInstance, object? value)
    {
        if (!CanWrite)
        {
            throw new InvalidOperationException("Field is read only.");
        }

        TargetMemberInfo.SetValue(objectInstance, value);
    }

    /// <summary>
    /// Invokes the underlying field represented by this class with the provided generic types and parameters.
    /// </summary>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="genericTypes">Any generic types used in the invoke process.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public override object? Invoke(object objectInstance, Type[]? genericTypes, object?[]? parameters)
    {
        if (genericTypes != null && genericTypes.Any())
        {
            throw new ArgumentException("Field invoke cannot accept generic types.");
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

        throw new ArgumentException("Setting the field can accept only 1 parameter.");
    }
}