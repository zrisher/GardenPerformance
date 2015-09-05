using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;

using SEGarden.Extensions;

using GP.Concealment.Messages.Requests;

namespace GP.Concealment {

    /// <summary>
    /// Runs on the client
    /// Checks for faction changes and lets the server know who's logged in and
    /// what faction they're in
    /// so it can properly reveal things needed for spawn
    /// </summary>
    class Player {

        private IMyPlayer IngamePlayer;

        public long Id { get; private set; }
        public long FactionId { get; private set; }
        
        public Player(IMyPlayer ingamePlayer) {
            IngamePlayer = ingamePlayer;
            Id = IngamePlayer.PlayerID;
            FactionId = GetFactionId();
        }

        public void Initialize() {
            LoginRequest request = new LoginRequest(Id, FactionId);
            request.SendToServer();
        }

        public void Update(){ 
            UpdateFaction();
        }

        public void Terminate(){
            LogoutRequest request = new LogoutRequest(Id, FactionId);
            request.SendToServer();
        }

        private void UpdateFaction() {
            long newFactionId = GetFactionId();

            if (newFactionId != FactionId) {
                FactionChangeRequest request = new FactionChangeRequest(
                    Id, FactionId, newFactionId);
                request.SendToServer();
                FactionId = newFactionId;
            }
        }

        private long GetFactionId(){
            IMyFaction faction = IngamePlayer.GetFaction();
            return (faction != null) ? faction.FactionId : 0;
        }

    }

}
