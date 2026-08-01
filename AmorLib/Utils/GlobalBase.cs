using GameData;
using LevelGeneration;
using System.Text.Json.Serialization;

namespace AmorLib.Utils;

public abstract class GlobalBase
{
    [JsonPropertyOrder(-10)]
    public eDimensionIndex DimensionIndex
    {
        get => _dimIndex;
        set { _dimIndex = value; MarkDirty(); }
    }
    private eDimensionIndex _dimIndex = eDimensionIndex.Reality;

    [JsonPropertyOrder(-10)]
    public LG_LayerType Layer
    {
        get => _layerType;
        set { _layerType = value; MarkDirty(); }
    }
    private LG_LayerType _layerType = LG_LayerType.MainLayer;

    [JsonPropertyOrder(-10)]
    public eLocalZoneIndex LocalIndex
    {
        get => _localIndex;
        set { _localIndex = value; MarkDirty(); }
    }
    private eLocalZoneIndex _localIndex = eLocalZoneIndex.Zone_0;

    [JsonIgnore]
    public GlobalZoneIndex GlobalZoneIndex { get { Refresh(); return _struct; } }
    private GlobalZoneIndex _struct;

    [JsonIgnore]
    public (int dimension, int layer, int zone) IntTuple { get { Refresh(); return _tuple; } }
    private (int dim, int layer, int zone) _tuple;

    public Dimension? Dimension;
    public LG_Zone? Zone;
    private bool _isDirty = true;

    protected GlobalBase()
    {
        Refresh();
        GlobalIndexUtil.BaseInstances.Add(new(this));
    }

    private void MarkDirty() => _isDirty = true;

    private void Refresh()
    {
        if (!_isDirty) return;
        _struct = GlobalIndexUtil.ToStruct(DimensionIndex, Layer, LocalIndex);
        _tuple = GlobalIndexUtil.ToIntTuple(DimensionIndex, Layer, LocalIndex);
        _isDirty = false;
    }

    public override string ToString() => $"({DimensionIndex}, {Layer}, {LocalIndex})";
}
