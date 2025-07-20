using System.Text.Json.Serialization;

namespace AmorLib.Utils.JsonElementConverters;

[JsonConverter(typeof(ValueBaseConverter))]
public struct ValueBase 
{    
    public float Value;
    public ValueMode Mode;
    public bool FromDefault;

    public ValueBase(float value = 1.0f, ValueMode mode = ValueMode.Rel, bool fromDefault = true)
    {
        Value = value;
        Mode = mode;
        FromDefault = fromDefault;
    }

    public readonly float GetAbsValue(float maxValue, float currentValue)
    {
        if (Mode == ValueMode.Rel)
        {
            return FromDefault ? currentValue * Value : maxValue * Value;
        }
        else
        {
            return Value;
        }
    }

    public readonly float GetAbsValue(float baseValue)
    {
        return Mode == ValueMode.Abs ? Value : baseValue * Value;
    }

    public readonly int GetAbsValue(int maxValue, int currentValue)
    {
        return (int)Math.Round(GetAbsValue((float)maxValue, currentValue));
    }

    public readonly int GetAbsValue(int baseValue)
    {
        return (int)Math.Round(GetAbsValue((float)baseValue));
    }
    
    public static readonly ValueBase Unchanged = new(1.0f, ValueMode.Rel, true);
    public static readonly ValueBase Zero = new(0.0f, ValueMode.Abs, false);

    public override readonly string ToString()
    {
        return $"[Mode: {Mode}, Value: {Value}]";
    }
}

public enum ValueMode
{
    Rel,
    Abs
}