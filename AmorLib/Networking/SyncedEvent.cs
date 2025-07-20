using GTFO.API;
using SNetwork;

namespace AmorLib.Networking;

public abstract class SyncedEvent<S> where S : struct
{
    public delegate void ReceiveHandler(S packet);

    public abstract string Prefix { get; }
    public abstract string GUID { get; }
    public bool IsSetup { get; private set; } = false;
    public string EventName { get; private set; } = string.Empty;

    public void Setup()
    {
        if (IsSetup)
            return;

        EventName = $"SE-{Prefix}-{GUID}";
        NetworkAPI.RegisterEvent<S>(EventName, ReceiveClient_Callback);
        IsSetup = true;
    }

    public void Send(S packetData, SNet_Player? target = null, SNet_ChannelType priority = SNet_ChannelType.GameNonCritical)
    {
        if (target is not null)
        {
            NetworkAPI.InvokeEvent(EventName, packetData, target, priority);
        }
        else
        {
            NetworkAPI.InvokeEvent(EventName, packetData, priority);
        }

        ReceiveLocal_Callback(packetData);
    }

    private void ReceiveLocal_Callback(S packet)
    {
        ReceiveLocal(packet);
        OnReceiveLocal?.Invoke(packet);
    }

    private void ReceiveClient_Callback(ulong sender, S packet)
    {
        Receive(packet);
        OnReceive?.Invoke(packet);
    }

    protected virtual void ReceiveLocal(S packet)
    {
    }

    protected virtual void Receive(S packet)
    {
    }

    public event ReceiveHandler? OnReceive;

    public event ReceiveHandler? OnReceiveLocal;
}