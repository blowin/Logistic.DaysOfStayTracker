namespace Logistic.DaysOfStayTracker.Core.Common;

public static class UpdateProperty
{
    public static UpdateProperty<T> Changed<T>(T value) => new(value, true);
    
    public static UpdateProperty<T?> ChangedNullable<T>(T value) where T : struct
        => new(value, true);
    
    public static UpdateProperty<T> NonChanged<T>() => new(default, false);
}

public sealed class UpdateProperty<T> : IEquatable<UpdateProperty<T>>
{
    public T? Value { get; private set; }
    public bool IsChanged { get; private set; }

    public UpdateProperty(T? value, bool isChanged)
    {
        Value = value;
        IsChanged = isChanged;
    }

    public void Change(T? value)
    {
        Value = value;
        IsChanged = true;
    }

    public void UnChange()
    {
        Value = default;
        IsChanged = false;
    }
    
    public bool Equals(UpdateProperty<T>? other) 
        => other != null && EqualityComparer<T>.Default.Equals(Value, other.Value) && IsChanged == other.IsChanged;

    public override bool Equals(object? obj) => obj is UpdateProperty<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Value, IsChanged);

    public override string ToString() => Value?.ToString() ?? "null";
}