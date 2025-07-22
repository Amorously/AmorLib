using UnityEngine;

namespace AmorLib.Networking;

public struct LowResColor
{
    public byte r;
    public byte g; 
    public byte b;
    public byte a;

    private static Color _color = Color.black;

    public static implicit operator Color(LowResColor lowResColor)
    {
        _color.r = lowResColor.r / 255.0f;
        _color.g = lowResColor.g / 255.0f;
        _color.b = lowResColor.b / 255.0f;
        _color.a = lowResColor.a / 255.0f;
        return _color;
    }

    public static implicit operator LowResColor(Color color)
    {
        return new()
        {
            r = (byte)(color.r * 255),
            g = (byte)(color.g * 255),
            b = (byte)(color.b * 255),
            a = (byte)(color.a * 255)
        };
    }
}