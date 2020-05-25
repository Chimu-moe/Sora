using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sora.Enums;
using Sora.EventArgs.BanchoEventArgs;
using ErrorStates = Sora.Enums.ErrorStates;
using Presence = Sora.Objects.Presence;

namespace Sora.Services
{
    public class PresenceService : PresenceKeeper
    {
        private readonly EventManager _ev;

        public PresenceService(EventManager ev) => _ev = ev;

        public int ConnectedPresences => Values.Count;

        public IEnumerable<int> GetUserIds(Presence pr = null)
        {
            try
            {
                RWL.AcquireReaderLock(1000); // this is already too long but who cares ?

                return Values.Where(x => x.Value != pr).Select(x => x.Value.User.Id);
            }
            finally
            {
                RWL.ReleaseReaderLock();
            }
        }

        public async void BeginTimeoutDetector()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    var toRemove = new List<Presence>();
                    try
                    {
                        RWL.AcquireReaderLock(1000);

                        foreach (var pr in Values.Select(presenceK => presenceK.Value)
                            .Where(pr => pr["BOT"] == null || !(bool) pr["BOT"]))
                        {
                            pr["LAST_PONG"] ??= DateTime.Now;

                            var lastPong = (DateTime) pr["LAST_PONG"];
                            if (lastPong < DateTime.Now - TimeSpan.FromSeconds(60))
                                toRemove.Add(pr);
                        }
                    }
                    finally
                    {
                        RWL.ReleaseReaderLock();
                    }

                    foreach (var removableValue in toRemove)
                        _ev?.RunEvent(EventType.BanchoExit, new BanchoExitArgs
                        {
                            Pr = removableValue,
                            Err = ErrorStates.Ok,
                        });

                    Thread.Sleep(1000);
                }
            });
        }
    }
}