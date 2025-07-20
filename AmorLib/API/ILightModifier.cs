using UnityEngine;

namespace AmorLib.API;

/// <summary>
/// A light modifier object. A light's color, intensity, and enabled state are affected by its modifiers (when active).
/// </summary>
public interface ILightModifier
{
    /// <summary>
    /// The color of this modifier.
    /// </summary>
    /// <value>Updates light color when modified if active.</value>
    public Color Color { get; set; }

    /// <summary>
    /// The intensity of this modifier.
    /// </summary>
    /// <value>Updates light intensity when modified if active.</value>
    public float Intensity { get; set; }

    /// <summary>
    /// The (light) enabled state of this modifer.
    /// </summary>
    /// <value>Updates light enabled state when modified if active.</value>
    public bool Enabled { get; set; }

    /// <summary>
    /// The priority of the modifier.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Whether the modifier is active on the light.
    /// </summary>
    public bool Active { get; }

    /// <summary>
    /// Sets a new color, intensity, and enabled state for the modifier if active.
    /// </summary>
    public void Set(Color color, float intensity, bool enabled)
    {
        Color = color;
        Intensity = intensity;
        Enabled = enabled;
    }

    /// <summary>
    /// Adds the modifier back to the top of the stack.
    /// </summary>
    /// <returns>False if the light no longer exists.</returns>
    public bool Register(); 

    /// <summary>
    /// Disables the modifier and removes it from the light.
    /// </summary>
    public void Remove();
}
