using GTFO.API;
using SNetwork;
using System.Runtime.InteropServices;

namespace AmorLib.Networking.StateReplicators;

public enum Size : int
{
    State4Byte = 4,
    State8Byte = 8,
    State16Byte = 16,
    State32Byte = 32,
    State48Byte = 48,
    State64Byte = 64,
    State80Byte = 80,
    State96Byte = 96,
    State128Byte = 128,
    State196Byte = 196,
    State256Byte = 256
}

internal delegate void OnReceiveDel<S>(ulong sender, uint replicatorID, S newState) where S : struct;

internal interface IReplicatorEvent<S>
{
    string EventName { get; }
    bool IsRegistered { get; }
    void Invoke(uint replicatorID, S data, SNet_Player? target = null, SNet_ChannelType priority = SNet_ChannelType.GameOrderCritical);
}

internal interface IStatePayload
{
    uint ID { get; set; }
    S Get<S>(Size size) where S : struct;
    void Set<S>(S stateData, Size size) where S : struct;
}

internal class ReplicatorPayload<S, P> : IReplicatorEvent<S> 
    where S : struct 
    where P : struct, IStatePayload
{
    public string EventName { get; private set; } = string.Empty;
    public bool IsRegistered => NetworkAPI.IsEventRegistered(EventName);
    private readonly Size _payloadSize;

    public ReplicatorPayload(Size size, string eventName, OnReceiveDel<S> onReceiveCallback)
    {
        NetworkAPI.RegisterEvent(eventName, (ulong sender, P payload) => onReceiveCallback?.Invoke(sender, payload.ID, payload.Get<S>(_payloadSize)));
        EventName = eventName;
        _payloadSize = size;
    }

    public void Invoke(uint replicatorID, S data, SNet_Player? target, SNet_ChannelType priority)
    {
        var payload = new P { ID = replicatorID };
        payload.Set(data, _payloadSize);

        if (target != null)
        {
            NetworkAPI.InvokeEvent(EventName, payload, target, priority);
        }
        else
        {
            NetworkAPI.InvokeEvent(EventName, payload, priority);
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct StatePayloadBytes : IStatePayload
{
    public uint ID { get; set; }
    public fixed byte PayloadBytes[256];

    public S Get<S>(Size size) where S : struct
    {
        int byteCount = (int)size;
        byte[] arr = new byte[byteCount];
        fixed (byte* p = PayloadBytes)
            Marshal.Copy((IntPtr)p, arr, 0, byteCount);
        return FromBytes<S>(arr);
    }

    public void Set<S>(S stateData, Size size) where S : struct
    {
        int byteCount = (int)size;
        byte[] buffer = ToBytes(stateData);
        if (buffer.Length > byteCount)
            throw new ArgumentException($"Data size {buffer.Length} exceeds payload size {byteCount}, unable to serialize {nameof(S)}!");

        fixed (byte* p = PayloadBytes)
        {
            Marshal.Copy(buffer, 0, (IntPtr)p, buffer.Length);
            for (int i = buffer.Length; i < byteCount; i++)
                p[i] = 0;
        }
    }

    private static byte[] ToBytes<S>(S data) where S : struct
    {
        int len = Marshal.SizeOf<S>();
        byte[] buffer = new byte[len];
        IntPtr ptr = Marshal.AllocHGlobal(len);
        try
        {
            Marshal.StructureToPtr(data, ptr, false);
            Marshal.Copy(ptr, buffer, 0, len);
            return buffer;
        }
        finally { Marshal.FreeHGlobal(ptr); }
    }

    private static S FromBytes<S>(byte[] arr) where S : struct
    {
        IntPtr ptr = Marshal.AllocHGlobal(arr.Length);
        try
        {
            Marshal.Copy(arr, 0, ptr, arr.Length);
            return Marshal.PtrToStructure<S>(ptr)!;
        }
        finally { Marshal.FreeHGlobal(ptr); }
    }
}