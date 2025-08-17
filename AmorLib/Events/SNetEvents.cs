using GTFO.API.Utilities;
using SNetwork;

namespace AmorLib.Events;

public static class SNetEvents
{
    public static event Action<eBufferType>? OnBufferCapture;

    public static event Action<eBufferType>? OnBufferRecall;    

    public static event Action? OnCheckpointReload;
    
    public static event Action? OnRecallDone;    

    internal static void BufferCaptured(eBufferType bufferType) => SafeInvoke.Invoke(OnBufferCapture, bufferType);
    internal static void BufferRecalled(eBufferType bufferType) => SafeInvoke.Invoke(OnBufferRecall, bufferType);
    internal static void CheckpointReloaded() => SafeInvoke.Invoke(OnCheckpointReload);
    internal static void RecallDone() => SafeInvoke.Invoke(OnRecallDone);
}
