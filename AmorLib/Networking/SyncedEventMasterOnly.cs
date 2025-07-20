using SNetwork;

namespace AmorLib.Networking;

public abstract class SyncedEventMasterOnly<S> : SyncedEvent<S> where S : struct
{
    public void Send(S packet, SNet_ChannelType priority = SNet_ChannelType.GameNonCritical)
    {
        if (!SNet.IsMaster)
            Send(packet, SNet.Master, priority);
        else
            Receive(packet);
    }
}