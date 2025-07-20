using System.Text.Json.Serialization;

namespace AmorLib.Utils.JsonElementConverters
{
    [JsonConverter(typeof(BoolBaseConverter))]
    public struct BoolBase
    {
        public BoolMode Mode;

        public BoolBase(bool mode)
        {
            Mode = mode ? BoolMode.True : BoolMode.False;
        }

        public BoolBase(BoolMode mode)
        {
            Mode = mode;
        }

        public readonly bool GetValue(bool originalValue)
        {
            return Mode switch
            {
                BoolMode.True => true,
                BoolMode.False => false,
                _ => originalValue
            };
        }
        
        public static readonly BoolBase False = new(BoolMode.False);
        public static readonly BoolBase True = new(BoolMode.True);
        public static readonly BoolBase Unchanged = new(BoolMode.Unchanged);
    }

    public enum BoolMode
    {
        False,
        True,
        Unchanged
    }
}
