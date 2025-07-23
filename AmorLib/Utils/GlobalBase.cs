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
        set 
        { 
            _dimIndex = value; 
            Dimension = Dimension.GetDimension(_dimIndex, out var d) ? d : null;
            RaiseFlag();
        }
    }
    private eDimensionIndex _dimIndex = eDimensionIndex.Reality;

    [JsonPropertyOrder(-10)]
    public LG_LayerType Layer
    {
        get => _layerType;
        set { _layerType = value; RaiseFlag(); }
    }
    private LG_LayerType _layerType = LG_LayerType.MainLayer;

    [JsonPropertyOrder(-10)]
    public eLocalZoneIndex LocalIndex
    {
        get => _localIndex;
        set { _localIndex = value; RaiseFlag(); }
    }
    private eLocalZoneIndex _localIndex = eLocalZoneIndex.Zone_0;

    [JsonIgnore]
    public GlobalZoneIndex GlobalZoneIndex
    {
        get { Refresh(); return _struct; }
    }
    private GlobalZoneIndex _struct;

    [JsonIgnore]
    public (int dimension, int layer, int zone) IntTuple
    {
        get { Refresh(); return _tuple; }
    }
    private (int dim, int layer, int zone) _tuple;

    [JsonIgnore]
    public Dimension? Dimension { get; private set; }
    [JsonIgnore]
    public LG_Zone? Zone { get; private set; }    
    private bool _updateFlag = true;

    protected GlobalBase()
    {
        Refresh();
    }

    private void RaiseFlag() => _updateFlag = true;

    private void Refresh()
    {
        if (!_updateFlag) return;
        _struct = GlobalIndexUtil.ToStruct(DimensionIndex, Layer, LocalIndex);
        _tuple = GlobalIndexUtil.ToIntTuple(DimensionIndex, Layer, LocalIndex);
        Zone = _tuple.TryGetZone(out var z) ? z : null;
        _updateFlag = false;
    }

    public override string ToString() => $"({DimensionIndex}, {Layer}, {LocalIndex})";
}
