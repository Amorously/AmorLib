using SNetwork;

namespace AmorLib.Events;

public static class SNetEvents
{
    public static event Action<eBufferType>? OnBufferCapture;

    public static event Action<eBufferType>? OnBufferRecall;    

    public static event Action? OnCheckpointReload;

    internal static void BufferCaptured(eBufferType bufferType) => OnBufferCapture?.Invoke(bufferType);
    internal static void BufferRecalled(eBufferType bufferType) => OnBufferRecall?.Invoke(bufferType);
    internal static void RecallDone() => OnCheckpointReload?.Invoke();
}
