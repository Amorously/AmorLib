using AmorLib.Events;
using GTFO.API;
using SNetwork;

namespace AmorLib.Networking.StateReplicators;

public struct Packet
{
    public uint replicatorID;
    public PacketAction action;
}

public enum PacketAction : byte
{
    Created,
    Destroyed,
    SyncRequest
}

internal sealed class ReplicatorHandshake
{
    public delegate void ClientRequestedSyncDel(SNet_Player requestedPlayer);
    public event ClientRequestedSyncDel? OnClientSyncRequested;

    public string EventName { get; private set; }
    public bool IsReadyToSync { get; private set; }

    private readonly Dictionary<uint, (bool SetupOnHost, bool SetupOnClient)> _lookup = new();

    public ReplicatorHandshake(string eventName)
    {
        EventName = eventName;
        NetworkAPI.RegisterEvent<Packet>(EventName, OnSyncAction);
        SNetEvents.OnCheckpointReload += OnRecallDone;
    }

    private void OnRecallDone()
    {
        Logger.Warn($"Handshake :: {EventName}, Client sending sync request");
        ClientSyncRequest();
    }

    private void ClientSyncRequest()
    {
        if (SNet.IsMaster) return;
        
        foreach (uint replicatorID in _lookup.Keys)
        {
            NetworkAPI.InvokeEvent(EventName, new Packet
            { 
                replicatorID = replicatorID, 
                action = PacketAction.SyncRequest
            }, SNet.Master, SNet_ChannelType.GameOrderCritical);
        }
    }

    public void Reset()
    {
        _lookup.Clear();
    }

    private void OnSyncAction(ulong sender, Packet packet)
    {
        if (!SNet.IsMaster && sender == SNet.Master.Lookup)
        {
            if (packet.action != PacketAction.SyncRequest)
            {
                SetHostState(packet.replicatorID, packet.action == PacketAction.Created);
            }
            else
            {
                Logger.Warn("Handshake :: OnSyncAction host sync request?");
            }
        }
        else if (SNet.IsMaster)
        {
            switch (packet.action)
            {
                case PacketAction.Created:
                    SetClientState(packet.replicatorID, true);
                    break;

                case PacketAction.Destroyed:
                    SetClientState(packet.replicatorID, false);
                    break;

                case PacketAction.SyncRequest:
                    if (!SNet.TryGetPlayer(sender, out var player))
                    {
                        Logger.Error($"Handshake :: Cannot find player from sender: {sender}");
                        break;
                    }
                    OnClientSyncRequested?.Invoke(player);
                    break;

                default:
                    Logger.Error($"Handshake :: Unknown packet action {packet.action}");
                    break;
            }
        }
    }

    public void UpdateCreated(uint id)
    {
        if (SNet.IsInLobby)
        {
            if (SNet.IsMaster)
            {
                SetHostState(id, true);
                NetworkAPI.InvokeEvent(EventName, new Packet
                {
                    replicatorID = id,
                    action = PacketAction.Created
                });
            }
            else if (SNet.HasMaster)
            {
                SetClientState(id, true);
                NetworkAPI.InvokeEvent(EventName, new Packet
                {
                    replicatorID = id,
                    action = PacketAction.Created
                }, SNet.Master);
            }
            else
            {
                Logger.Error("Handshake :: MASTER is NULL in lobby; this should NOT happen!!");
            }
        }
        else
        {
            Logger.Error("Handshake :: Session LifeTimeType StateReplicator cannot be created without lobby!");
        }
    }

    public void UpdateDestroyed(uint id)
    {
        if (SNet.IsInLobby)
        {
            if (SNet.IsMaster)
            {
                SetHostState(id, true);
                NetworkAPI.InvokeEvent(EventName, new Packet
                {
                    replicatorID = id,
                    action = PacketAction.Destroyed
                });
            }
            else if (SNet.HasMaster)
            {
                SetClientState(id, true);
                NetworkAPI.InvokeEvent(EventName, new Packet
                {
                    replicatorID = id,
                    action = PacketAction.Destroyed
                }, SNet.Master);
            }
            else
            {
                Logger.Error("Handshake :: MASTER is NULL in lobby; this should NOT happen!!");
            }
        }
        else
        {
            Logger.Error("Handshake :: Session LifeTimeType StateReplicator cannot be created without lobby!");
        }
    }

    private void SetHostState(uint id, bool isSetup)
    {
        if (_lookup.TryGetValue(id, out var data))
        {
            data.SetupOnHost = isSetup;
        }
        else
        {
            _lookup[id] = new() { SetupOnHost = isSetup };
        }

        UpdateSyncState(id);
    }

    private void SetClientState(uint id, bool isSetup)
    {
        if (_lookup.TryGetValue(id, out var data))
        {
            data.SetupOnClient = isSetup;
        }
        else
        {
            _lookup[id] = new() { SetupOnClient = isSetup };
        }

        UpdateSyncState(id);
    }

    private void UpdateSyncState(uint id)
    {
        var isReadyOld = IsReadyToSync;
        IsReadyToSync = _lookup.TryGetValue(id, out var data) && data.SetupOnHost && data.SetupOnClient;

        if (IsReadyToSync && IsReadyToSync != isReadyOld && SNet.HasMaster && !SNet.IsMaster)
        { 
            NetworkAPI.InvokeEvent(EventName, new Packet
            {
                replicatorID = id,
                action = PacketAction.SyncRequest
            }, SNet.Master);
        }
    }
}