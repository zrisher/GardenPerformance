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
        public List<Player> LocalPlayers = new List<Player>();

        public override string ComponentName { get { return "ClientConcealSession"; } }

        public override Dictionary<uint, Action> UpdateActions {
            get {
                var actions = base.UpdateActions;
                actions.Add(3600, UpdatePlayers);
                return actions;
            }
        }

        public override void Initialize() {
            Log.Trace("Initializing Client Conceal Session", "Initialize");
            GardenGateway.Commands.addCommands(Commands.FullTree);
            Messenger = new ClientMessageHandler();
            ModMessenger = new ModMessageHandler();
            BuildPlayerList();
            new SettingsRequest().SendToServer();
            Instance = this;
            Log.Trace("Finished Initializing Client Conceal Session", "Initialize");
        }

        private void UpdatePlayers() {
            foreach (Player player in LocalPlayers) {
                player.Update();
            }
        }

        public override void Terminate() {
            Log.Trace("Terminating Client Conceal Session", "Terminate");
            TerminatePlayerList();
            Instance = null;
            Log.Trace("Finished Terminate Client Conceal Session", "Terminate");
        }

        private void BuildPlayerList() {
            LocalSteamId = MyAPIGateway.Multiplayer.MyId;

            Log.Trace("Start building local player list for local steam id " + 
                LocalSteamId, "BuildPlayerList");

            var localIngamePlayers = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(localIngamePlayers, (x) =>
                x.SteamUserId == LocalSteamId
            );

            if (localIngamePlayers.Count == 0) {
                Log.Error("Failed to find player for my steamId" + LocalSteamId, "Initialize");
            }

            Log.Trace("Building " + localIngamePlayers.Count + " players.", "BuildPlayerList");

            foreach (IMyPlayer ingamePlayer in localIngamePlayers) {
                Player player = new Player(ingamePlayer);
                LocalPlayers.Add(player);
                player.Initialize();
            }
        }

        private void TerminatePlayerList() {
            Log.Trace("Terminating " + LocalPlayers.Count + " players.", "BuildPlayerList");
            foreach (Player player in LocalPlayers) {
                player.Terminate();
            }
        }


    }

}
