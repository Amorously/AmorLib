using AmorLib.Events;
using GTFO.API;
using SNetwork;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace AmorLib.Networking.StateReplicators;

public sealed partial class StateReplicator<S> where S : struct
{
    public static readonly string Name, HashName, ClientRequestEventName, HostSetStateEventName, HostSetRecallStateEventName, HandshakeEventName;
    public static readonly int StateSize;
    public static readonly Size StateSizeType;

    private static readonly IReplicatorEvent<S>? _clientRequestEvent, _hostSetStateEvent, _hostSetRecallStateEvent;
    private static readonly ReplicatorHandshake? _handshake;

    internal static readonly Dictionary<uint, StateReplicator<S>> _replicators = new();

    static StateReplicator()
    {
        Name = typeof(S).Name;
        StateSize = Marshal.SizeOf(typeof(S));
        StateSizeType = GetSizeType(StateSize);

        using var md5 = MD5.Create();
        byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(typeof(S).FullName!));
        HashName = Convert.ToBase64String(bytes);
        ClientRequestEventName = $"SRcr-{Name}-{HashName}";
        HostSetStateEventName = $"SRhs-{Name}-{HashName}";
        HostSetRecallStateEventName = $"SRre-{Name}-{HashName}";
        HandshakeEventName = $"RH-{Name}-{HashName}";

        _clientRequestEvent = CreatePayloadEvent(StateSizeType, ClientRequestEventName, ClientRequestEventCallback);
        _hostSetStateEvent = CreatePayloadEvent(StateSizeType, HostSetStateEventName, HostSetStateEventCallback);
        _hostSetRecallStateEvent = CreatePayloadEvent(StateSizeType, HostSetRecallStateEventName, HostSetRecallStateEventCallback);
        _handshake = CreateHandshakeEvent(HandshakeEventName);

        SNetEvents.OnBufferCapture += BufferStored;
        SNetEvents.OnBufferRecall += BufferRecalled;
        LevelAPI.OnLevelCleanup += OnLevelCleanup;
    }    

    /// <summary>
    /// Create a new <see cref="StateReplicator{S}"/> instance of type <typeparamref name="S"/> with the specified ID, starting state, and lifetime type.
    /// </summary>
    /// <remarks>
    /// Validates <paramref name="replicatorID"/> to ensure it's non-zero and not already assigned.
    /// If <paramref name="lifeTime"/> is <see cref="LifeTimeType.Permanent"/>, handshaking is disabled.
    /// </remarks>
    /// <returns>A new <see cref="StateReplicator{S}"/> instance if the successful; otherwise, <see langword="null"/>.</returns>
    public static StateReplicator<S>? Create(uint replicatorID, S startState, LifeTimeType lifeTime)
    {
        if (replicatorID == 0u)
        {
            Logger.Error("Replicator ID 0 is reserved for empty!");
            return null;
        }

        if (_replicators.ContainsKey(replicatorID))
        {
            Logger.Error("Replicator ID has already assigned!");
            return null;
        }
        
        var replicator = new StateReplicator<S>
        {
            ID = replicatorID,
            LifeTime = lifeTime,
            State = startState
        };

        if (lifeTime == LifeTimeType.Permanent)
        {
            Logger.Debug($"LifeTime is {nameof(LifeTimeType.Permanent)} :: Handshaking is disabled");
        }
        else if (lifeTime == LifeTimeType.Session)
        {
            _handshake?.UpdateCreated(replicatorID);
        }
        else
        {
            Logger.Error($"LifeTime {lifeTime} is invalid!");
            return null;
        }

        _replicators[replicatorID] = replicator;
        return replicator;
    }

    public static Size GetSizeType(int stateSize)
    {
        Size highestSizeCap = Size.State8Byte;
        foreach (Size sizeType in Enum.GetValues(typeof(Size)))
        {
            if (stateSize <= (int)sizeType && highestSizeCap < sizeType)
            {
                highestSizeCap = sizeType;
                break;
            }
        }

        return highestSizeCap;
    }

    private static IReplicatorEvent<S>? CreatePayloadEvent(Size size, string name, OnReceiveDel<S> callback)
    {
        if (NetworkAPI.IsEventRegistered(name))
        {
            Logger.Error($"ReplicatorPayload<{Name}>.{nameof(callback)} failed to initialize: Event name {name} is already registered!");
            return null;
        }

        return new ReplicatorPayload<S, StatePayloadBytes>(size, name, callback);
    }

    private static ReplicatorHandshake? CreateHandshakeEvent(string name)
    {
        if (NetworkAPI.IsEventRegistered(name))
        {
            Logger.Error($"ReplicatorHandshake<{Name}> failed to initialize: Event name {name} is already registered!");
            return null;
        }

        var handshake = new ReplicatorHandshake(name);
        handshake.OnClientSyncRequested += ClientSyncRequested;
        return handshake;
    }

    private static void ClientRequestEventCallback(ulong sender, uint replicatorID, S newState)
    {
        if (!SNet.IsMaster)
            return;

        if (_replicators.TryGetValue(replicatorID, out var replicator))
        {
            replicator.SetState(newState);
        }
    }

    private static void HostSetStateEventCallback(ulong sender, uint replicatorID, S newState)
    {
        if (!SNet.HasMaster)
            return;

        if (SNet.Master.Lookup != sender)
            return;

        if (_replicators.TryGetValue(replicatorID, out var replicator))
        {
            replicator.Internal_ChangeState(newState, isRecall: false);
        }
    }

    private static void HostSetRecallStateEventCallback(ulong sender, uint replicatorID, S newState)
    {
        if (!SNet.HasMaster)
            return;

        if (SNet.Master.Lookup != sender)
            return;

        if (_replicators.TryGetValue(replicatorID, out var replicator))
        {
            replicator.Internal_ChangeState(newState, isRecall: true);
        }
    }

    private static void ClientSyncRequested(SNet_Player requestedPlayer)
    {
        foreach (var replicator in _replicators.Values)
        {
            if (replicator.IsValid)
            {
                replicator.SendDropInState(requestedPlayer);
            }
        }
    }

    private static void BufferStored(eBufferType type)
    {
        foreach (var replicator in _replicators.Values)
        {
            if (replicator.IsValid)
            {
                replicator.SaveSnapshot(type);
            }
        }
    }

    private static void BufferRecalled(eBufferType type)
    {
        foreach (var replicator in _replicators.Values)
        {
            if (replicator.IsValid)
            {
                replicator.RestoreSnapshot(type);
            }
        }
    }

    private static void OnLevelCleanup()
    {
        UnloadSessionReplicators();
    }

    public static void UnloadSessionReplicators()
    {
        List<uint> idsToRemove = new();
        foreach (var replicator in _replicators.Values)
        {
            if (replicator.LifeTime == LifeTimeType.Session)
            {
                idsToRemove.Add(replicator.ID);
                replicator.Unload();
            }
        }

        foreach (var id in idsToRemove)
        {
            _replicators.Remove(id);
        }

        _handshake?.Reset();
    }
}