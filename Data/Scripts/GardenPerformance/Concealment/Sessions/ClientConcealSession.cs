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
using SEGarden.Logic.Common;
//using SEGarden.Notifications;

using GP.Concealment.Messaging.Handlers;

namespace GP.Concealment.Sessions {

    class ClientConcealSession {

        private static Logger Log = 
            new Logger("GardenPerformance.Concealment.Sessions.ClientConcealSession");

        public static ClientMessageHandler Messenger;

        public List<Records.Entities.ConcealableGrid> RevealedGrids;
        public List<Records.Entities.ConcealableGrid> ConcealedGrids;

        public void Initialize() {
            Log.Trace("Initializing Client Conceal Session", "Initialize");
            GardenGateway.Commands.addCommands(Commands.FullTree);
            Messenger = new ClientMessageHandler();
            Log.Trace("Finished Initializing Client Conceal Session", "Initialize");
        }

        public void Terminate() {
            Log.Trace("Terminating Client Conceal Session", "Initialize");
            Log.Trace("Finished Initializing Client Conceal Session", "Initialize");
        }

    }

}
