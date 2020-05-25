using System.Collections.Generic;
using System.IO;
using Sora.Attributes;
using Sora.Enums;
using Sora.EventArgs.BanchoEventArgs;
using Hex = Sora.Utilities.Hex;
using MessageStruct = Sora.Packets.Server.MessageStruct;
using Presence = Sora.Objects.Presence;

namespace Sora.Bot.Commands
{
    [EventClass]
    public class DebugCommand : ISoraCommand
    {
        private readonly EventManager _ev;
        public string Command => "debug";
        public string Description => "Debug packets";

        public List<Argument> Args => new List<Argument>();

        public int ExpectedArgs => 0;

        public Permission RequiredPermission => Permission.From(Permission.GROUP_DEVELOPER);

        public DebugCommand(EventManager ev) => _ev = ev;

        public bool Execute(Presence executor, string[] args)
        {
            if (executor["IS_PACKET_DEBUGGING"] == null)
                executor["IS_PACKET_DEBUGGING"] = false;

            executor["IS_PACKET_DEBUGGING"] = !(bool) executor["IS_PACKET_DEBUGGING"];

            executor.Alert("Debugger has been " + ((bool) executor["IS_PACKET_DEBUGGING"] ? "Enabled" : "Disabled"));

            return false;
        }

        [Event(EventType.BanchoPacket)]
        public void OnBanchoPacketEvent(BanchoPacketArgs args)
        {
            if (args.Pr["IS_PACKET_DEBUGGING"] == null)
                return;

            if ((bool) args.Pr["IS_PACKET_DEBUGGING"])
                _ev?.RunEvent(EventType.BanchoSendIrcMessagePrivate, new BanchoSendIrcMessageArgs
                {
                    Pr = args.Pr,
                    Message = new MessageStruct
                    {
                        Message = $"\n\n\n\n\n\n\n\n\n\n" +
                                  $"\nPacketId: {args.PacketId}" +
                                  $"\nPacket Length: {args.Data.BaseStream.Length}" +
                                  $"\nPacketData: {Hex.ToHex(((MemoryStream) args.Data.BaseStream).ToArray())}",
                        Username = args.Pr.User.UserName,
                        ChannelTarget = args.Pr.User.UserName,
                        SenderId = args.Pr.User.Id,
                    },
                });
        }
    }
}