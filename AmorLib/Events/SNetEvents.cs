using GTFO.API.Utilities;
using SNetwork;

namespace AmorLib.Events;

public static class SNetEvents
{
    public static event Action<eBufferType>? OnBufferCapture;

    public static event Action<eBufferType>? OnBufferRecall;    
    
    public static event Action? OnRecallDone;

    [Obsolete("Implemented in GTFO-API")]
    public static event Action? OnCheckpointReached;

    [Obsolete("Implemented in GTFO-API")]
    public static event Action? OnCheckpointReload;        

    internal static void BufferCaptured(eBufferType bufferType) => SafeInvoke.Invoke(OnBufferCapture, bufferType);
    internal static void BufferRecalled(eBufferType bufferType) => SafeInvoke.Invoke(OnBufferRecall, bufferType);
    internal static void RecallDone() => SafeInvoke.Invoke(OnRecallDone);
    internal static void CheckpointReached() => SafeInvoke.Invoke(OnCheckpointReached);
    internal static void CheckpointReloaded() => SafeInvoke.Invoke(OnCheckpointReload);    
}
