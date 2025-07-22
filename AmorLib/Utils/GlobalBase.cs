using GameData;
using LevelGeneration;
using System.Text.Json.Serialization;

namespace AmorLib.Utils;

public class GlobalBase
{
    [JsonPropertyOrder(-10)]
    public eDimensionIndex DimensionIndex { get; set; } = eDimensionIndex.Reality;

    [JsonPropertyOrder(-10)]
    public LG_LayerType Layer { get; set; } = LG_LayerType.MainLayer;

    [JsonPropertyOrder(-10)]
    public eLocalZoneIndex LocalIndex { get; set; } = eLocalZoneIndex.Zone_0;
    
    [JsonIgnore]
    public GlobalZoneIndex GlobalZoneIndex
    {
        get
        {
            if (_gzi.Dimension != DimensionIndex || _gzi.Layer != Layer || _gzi.Zone != LocalIndex)
            {
                _gzi = GlobalIndexUtil.ToStruct(DimensionIndex, Layer, LocalIndex);
            }
            return _gzi;
        }
        private set => _gzi = value;
    }
    private GlobalZoneIndex _gzi;

    [JsonIgnore]
    public (int dimension, int layer, int zone) IntTuple
    {
        get
        {
            if (_intTuple.dim != (int)DimensionIndex || _intTuple.layer != (int)Layer || _intTuple.zone != (int)LocalIndex)
            {
                _intTuple = GlobalIndexUtil.ToIntTuple(DimensionIndex, Layer, LocalIndex);
            }
            return _intTuple;
        }
        private set => _intTuple = value; 
    }
    private (int dim, int layer, int zone) _intTuple;

    public GlobalBase()
    {
        _gzi = GlobalIndexUtil.ToStruct(DimensionIndex, Layer, LocalIndex);
        _intTuple = GlobalIndexUtil.ToIntTuple(DimensionIndex, Layer, LocalIndex);
    }

    public override string ToString()
    {
        return $"({DimensionIndex}, {Layer}, {LocalIndex})";
    }
}
