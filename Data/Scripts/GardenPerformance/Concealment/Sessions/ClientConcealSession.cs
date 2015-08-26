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

namespace GP.Concealment.Sessions {

    class ClientConcealSession : SessionComponent {

        private static Logger Log = 
            new Logger("GardenPerformance.Concealment.Sessions.ClientConcealSession");

        public static ClientMessageHandler Messenger;
        public static ClientConcealSession Instance;

        public List<ConcealedGrid> RevealedGrids;
        public List<ConcealedGrid> ConcealedGrids;

        public String Name { get { return "ClientConcealSession"; } }

        public override void Initialize() {
            Log.Trace("Initializing Client Conceal Session", "Initialize");
            GardenGateway.Commands.addCommands(Commands.FullTree);
            Messenger = new ClientMessageHandler();
            Instance = this;
            Log.Trace("Finished Initializing Client Conceal Session", "Initialize");

            // Send message to server letting them know we joined, so they
            // know to hold our spawnpoints for us
        }

        public override void Terminate() {
            Log.Trace("Terminating Client Conceal Session", "Terminate");
            Instance = null;
            Log.Trace("Finished Terminate Client Conceal Session", "Terminate");
        }

    }

}
