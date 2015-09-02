using System;
using System.Collections.Generic;
using System.Text;

using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Library.Utils;
using Interfaces = Sandbox.ModAPI.Interfaces;
using InGame = Sandbox.ModAPI.Ingame;

using SEGarden;
using SEGarden.Logging;
using SEGarden.Logic;


//using SEGarden.Notifications;

using GP.Concealment.MessageHandlers;
using GP.Concealment.World.Entities;
using GP.Concealment.Messages.Requests;

namespace GP.Concealment.Sessions {

    class ClientConcealSession : SessionComponent {

        private static Logger Log = 
            new Logger("GardenPerformance.Concealment.Sessions.ClientConcealSession");

        public static ClientMessageHandler Messenger;
        public static ModMessageHandler ModMessenger;
        public static ClientConcealSession Instance;

        public List<RevealedGrid> RevealedGrids;
        public List<ConcealedGrid> ConcealedGrids;
        public Settings Settings;
        public List<ObservingEntity> ObservingEntities;
        public ulong LocalSteamId;
        public long LocalPlayerId;

        public override string ComponentName { get { return "ClientConcealSession"; } }

        public override void Initialize() {
            Log.Trace("Initializing Client Conceal Session", "Initialize");
            GardenGateway.Commands.addCommands(Commands.FullTree);
            Messenger = new ClientMessageHandler();
            ModMessenger = new ModMessageHandler();

            LocalSteamId = MyAPIGateway.Multiplayer.MyId;

            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(players, (x) =>
                x.SteamUserId == LocalSteamId
            );

            if (players.Count == 0) {
                Log.Error("Failed to find player for my steamId" + LocalSteamId, "Initialize");
            }
            else if (players.Count > 1) {
                Log.Error("Found more than one player for steamId " + LocalSteamId, "Initialize");
            }
            else {
                LocalPlayerId = players[0].PlayerID;
                // Tell server we're here so it can reveal our spawn points
                // I couldn't find any existing events for playerLoggedIn
                LoginRequest request = new LoginRequest(LocalPlayerId);
                request.SendToServer();
            }

            Instance = this;
            Log.Trace("Finished Initializing Client Conceal Session", "Initialize");

            // Send message to server letting them know we joined, so they
            // know to hold our spawnpoints for us
        }

        public override void Terminate() {
            Log.Trace("Terminating Client Conceal Session", "Terminate");

            if (LocalPlayerId != 0) {
                // Tell server we're leaving so it can conceal our spawn points
                // I couldn't find any existing events for playerLoggedOut
                LogoutRequest request = new LogoutRequest(LocalPlayerId);
                request.SendToServer();
            }

            Instance = null;
            Log.Trace("Finished Terminate Client Conceal Session", "Terminate");
        }

    }

}
