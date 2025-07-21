using AIGraph;
using AmorLib.Utils.Extensions;
using LevelGeneration;
using UnityEngine;

namespace AmorLib.API;

public static class LightPriority
{
    public const int Normal = 1000;
    public const int EMP = 2000;
}

public class LightWorker
{
    public readonly LG_Zone OwnerZone;
    public readonly AIG_CourseNode SpawnNode;
    public readonly LG_Light Light;
    public readonly int InstanceID;
    public readonly float PrefabIntensity;
    public LG_LightAnimator? Animator;
    public Color OrigColor;    
    public float OrigIntensity;
    public bool OrigEnabled;

    private readonly SortedDictionary<int, LinkedList<LightModifier>> _priorityDict = new();
    private LinkedListNode<LightModifier>? _currentNode;
    public const int OrigPriority = -1000;
    private ILightModifier _origLightMod = null!;

    public Vector3 Position => Light.GetPosition();

    public LightWorker(LG_Zone zone, AIG_CourseNode node, LG_Light light, int id, float intensity)
    {
        OwnerZone = zone;
        SpawnNode = node;
        Light = light;
        InstanceID = id;
        PrefabIntensity = intensity;
    }

    internal void Setup()
    {        
        Animator = Light.gameObject.GetComponent<LG_LightAnimator>();
        OrigColor = Light.m_color;        
        OrigIntensity = Light.m_intensity;
        OrigEnabled = Light.gameObject.active;
        _origLightMod = AddModifier(OrigColor, OrigIntensity, OrigEnabled, OrigPriority);
    }
    
    public bool IsEMPActive() => _priorityDict.TryGetValue(LightPriority.EMP, out var stack) && stack.Count > 0;

    private void ChangeLightColor(Color color) => Light.ChangeColor(color);

    private void ChangeLightIntensity(float intensity) => Light.ChangeIntensity(intensity);

    private void SetLightEnabled(bool enabled) => Light.SetEnabled(enabled);

    /// <summary>
    /// Toggles the vanilla light flicker animation.
    /// </summary>
    /// <returns><see langword="True"/> if the light had a <see cref="LG_LightAnimator"/> component.</returns>
    public bool ToggleLightFlicker(bool enabled)
    {
        if (Animator == null) return false;

        var c_Light = Light.GetC_Light();
        if (enabled)
        {
            Animator.ResetRamp(c_Light);
        }
        else
        {
            Animator.m_inRamp = false;
            Animator.m_startTime = float.MaxValue;
            Animator.m_absTime = 1.0f;
            Animator.FeedLight(c_Light);
        }
        return true;
    }

    /// <summary>
    /// Adds a local light modifier.
    /// </summary>
    /// <returns>The modifier object created.</returns>
    public ILightModifier AddModifier(Color color, float intensity, bool enabled, int priority = LightPriority.Normal)
    {
        LightModifier modifier = new(color, intensity, enabled, priority, this);
        AddModifier(modifier);
        return modifier;
    }

    private void AddModifier(LightModifier modifier)
    {
        var stack = _priorityDict.GetOrAddNew(modifier.Priority);
        modifier.Node = stack.AddLast(modifier);
        if (_currentNode == null || _currentNode.Value.Priority <= modifier.Priority)
        {
            _currentNode = modifier.Node;
            modifier.Apply();
        }
    }

    private void RemoveModifier(LightModifier modifier)
    {
        var stack = _priorityDict[modifier.Priority];
        if (_currentNode != modifier.Node)
        {
            stack.Remove(modifier.Node!);
            modifier.Node = null;
            return;
        }

        if (modifier.Node!.Previous != null)
        {
            _currentNode = modifier.Node.Previous;
        }
        else
        {
            _currentNode = _priorityDict.Values.Last(list => list.Count > 0).Last;
        }

        stack.Remove(modifier.Node);
        modifier.Node = null;
        _currentNode!.Value.Apply();
    }

    // If necessary, e.g. cleaning up or resetting lights, might not need it
    internal void Reset() 
    {
        foreach (var stack in _priorityDict.Values)
        {
            foreach (var modifier in stack)
            {
                modifier.Remove();
            }
        }

        _priorityDict.Clear();
        _origLightMod.Register();
    }

    class LightModifier : ILightModifier
    {
        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                if (value == _color) return;

                _color = value;
                if (InUse) 
                    _worker.ChangeLightColor(_color);
            }
        }

        private float _intensity;
        public float Intensity
        {
            get => _intensity;
            set
            {
                if (value == _intensity) return;

                _intensity = value;
                if (InUse) 
                    _worker.ChangeLightIntensity(_intensity);
            }
        }

        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value == _enabled) return;

                _enabled = value;
                if (InUse) 
                    _worker.SetLightEnabled(_enabled);
            }
        }

        public int Priority { get; }       
        public LinkedListNode<LightModifier>? Node { get; set; } 
        public bool Active => Node != null && _worker != null;
        private bool InUse => Active && _worker._currentNode == Node;
        private readonly LightWorker _worker;

        public LightModifier(Color color, float intensity, bool enabled, int priority, LightWorker worker)
        {
            _color = color;
            _intensity = intensity;
            _enabled = enabled;
            Priority = priority;
            _worker = worker;
        }        

        public bool Register()
        {
            if (_worker == null) return false;
            else if (Active) _worker._priorityDict[Priority].Remove(Node!);

            _worker.AddModifier(this);
            return true;
        }
        
        public void Apply()
        {
            _worker.ChangeLightColor(_color);
            _worker.ChangeLightIntensity(_intensity);
            _worker.SetLightEnabled(_enabled);
        }

        public void Remove()
        {
            if (!Active) return;

            _worker.RemoveModifier(this);
        }
    }
}
