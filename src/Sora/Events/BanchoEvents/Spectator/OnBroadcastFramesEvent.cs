using Sora.Attributes;
using Sora.Enums;
using Sora.EventArgs.BanchoEventArgs;
using SpectatorFrames = Sora.Packets.Server.SpectatorFrames;

namespace Sora.Events.BanchoEvents.Spectator
{
    [EventClass]
    public class OnBroadcastFramesEvent
    {
        [Event(EventType.BanchoBroadcastFrames)]
        public void OnBroadcastFrames(BanchoBroadcastFramesArgs args)
        {
            if (args.Pr.Spectator?.Host != args.Pr)
                return;

            args.Pr.Spectator?.Push(new SpectatorFrames(args.Frames));
        }
    }
}