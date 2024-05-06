﻿namespace Rosidl.Generator.CSharp;

public abstract record FieldMetadata(
    TypeMetadata Type,
    string Name,
    string[] Comments
);

public record VariableFieldMetadata(TypeMetadata Type, string Name, object? DefaultValue, string[] Comments) : FieldMetadata(Type, Name, Comments)
{
    public override string ToString()
        => DefaultValue is null ? $"{Type} {Name}" : $"{Type} {Name} {ToString(DefaultValue)}";

    private static string ToString(object value)
    {
        if (value is string sv)
        {
            return string.Format("\"{0}\"", sv.Escape());
        }
        else if (value is bool bv)
        {
            return bv ? "true" : "false";
        }
        else if (value is object[] o)
        {
            return $"[{string.Join(", ", o.Select(ToString))}]";
        }
        else
        {
            return value.ToString() ?? string.Empty;
        }
    }
}

public record ConstantFieldMetadata(TypeMetadata Type, string Name, object Value, string[] Comments) : FieldMetadata(Type, Name, Comments)
{
    public override string ToString()
        => $"{Type} {Name} = {Value}";
}