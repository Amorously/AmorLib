using SNetwork;

namespace AmorLib.Networking.StateReplicators;

public enum LifeTimeType
{
    Permanent,
    Session
}

public sealed partial class StateReplicator<S> where S : struct
{
    public bool IsValid => ID != 0u;
    public bool IsInvalid => ID == 0u;
    public uint ID { get; private set; }
    public LifeTimeType LifeTime { get; private set; }
    public IStateReplicatorHolder<S>? Holder { get; private set; }
    public S State { get; private set; }
    public S LastState { get; private set; }
    public bool ClientSendStateAllowed { get; set; } = true;
    public bool CanSendToClient => SNet.IsInLobby && SNet.IsMaster;
    public bool CanSendToHost => SNet.IsInLobby && !SNet.IsMaster && SNet.HasMaster && ClientSendStateAllowed;

    internal readonly Dictionary<eBufferType, S> _recallStateSnapshots = new();

    public event Action<S, S, bool>? OnStateChanged;

    /// <summary>
    /// Updates and syncs a new state.
    /// </summary>
    public void SetState(S state)
    {
        if (IsInvalid) return;

        DoSync(state);
    }

    /// <summary>
    /// Updates but does not sync a new state.
    /// </summary>
    public void SetStateUnsynced(S state)
    {
        if (IsInvalid) return;

        LastState = state;
        State = state;
    }

    /// <summary>
    /// Unloads this replicator, clears recall snapshots, and removes handshaking.
    /// </summary>
    public void Unload()
    {
        if (IsValid)
        {
            _replicators.Remove(ID);
            _recallStateSnapshots.Clear();
            _handshake?.UpdateDestroyed(ID);
            ID = 0u;
        }
    }

    private void DoSync(S newState)
    {
        if (IsValid)
        {
            if (CanSendToClient)
            {
                _hostSetStateEvent?.Invoke(ID, newState);
                Internal_ChangeState(newState, false);
            }
            else if (CanSendToHost)
            {
                _clientRequestEvent?.Invoke(ID, newState, SNet.Master);
            }
        }
    }

    private void Internal_ChangeState(S state, bool isRecall)
    {
        if (IsInvalid) return;

        var oldState = State;
        State = state;
        LastState = oldState;

        OnStateChanged?.Invoke(oldState, state, isRecall);
        Holder?.OnStateChange(oldState, state, isRecall);
    }

    private void SendDropInState(SNet_Player target)
    {
        if (IsInvalid || target == null)
        {
            Logger.Error($"IsInvalid: {IsInvalid}, {nameof(SendDropInState)} :: Target was null? {target == null}?");
            return;
        }

        _hostSetRecallStateEvent?.Invoke(ID, State, target);
    }

    public void ClearAllRecallSnapshot()
    {
        if (IsInvalid) return;

        _recallStateSnapshots.Clear();
    }

    private void SaveSnapshot(eBufferType type)
    {
        if (IsInvalid) return;

        _recallStateSnapshots[type] = State;
    }

    private void RestoreSnapshot(eBufferType type)
    {
        if (IsValid && CanSendToClient)
        {
            if (_recallStateSnapshots.TryGetValue(type, out var savedState))
            {
                _hostSetRecallStateEvent?.Invoke(ID, savedState);
                Internal_ChangeState(savedState, isRecall: true);
            }
            else
            {
                Logger.Error($"{nameof(RestoreSnapshot)} :: There was no snapshot for {type}?");
            }
        }
    }
}