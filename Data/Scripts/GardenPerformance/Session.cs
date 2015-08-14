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

using GardenPerformance.Concealment.Sessions;

namespace GardenPerformance {

    // TODO: Server vs Client vs Singleplayer logic
    // various classes for each? Handle it all from here?
    // Until GardenSession is managed by update manager, we'd have to have 3 
    // registered sessions if we wanted to subclass it. So manage from one for now
    // TODO: Message processing in SEGarden

    /// <summary>
    /// LoadData, UnloadData, Update Before/After/Simulate, UpdatingStopped
    /// </summary>
	[Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.BeforeSimulation)]
    class Session : GardenSession {

        private static Logger Log = new Logger("GardenPerformance.Components");

        protected override RunLocation RunOn { get { return RunLocation.Any; } }

        private ClientConcealSession ClientConcealSession;
        private ServerConcealSession ServerConcealSession;

        protected override void Initialize() {
            base.Initialize();

            Log.Trace("Starting Conceal Sessions", "Initialize");

            switch (RunningOn) {
                case RunLocation.Client:
                    ClientConcealSession = new ClientConcealSession();
                    ClientConcealSession.Initialize();
                    break;
                case RunLocation.Server:
                    ServerConcealSession = new ServerConcealSession();
                    ServerConcealSession.Initialize();
                    break;
                case RunLocation.Singleplayer:
                    ServerConcealSession = new ServerConcealSession();
                    ServerConcealSession.Initialize();
                    ClientConcealSession = new ClientConcealSession();
                    ClientConcealSession.Initialize();
                    break;
            }

            Log.Trace("Finished Starting Conceal Sessions", "Initialize");
        }

        protected override void Terminate() {
            base.Terminate();

            Log.Trace("Terminating Conceal Sessions", "Initialize");

            switch (RunningOn) {
                case RunLocation.Client:
                    ClientConcealSession.Terminate();
                    ClientConcealSession = null;
                    break;
                case RunLocation.Server:
                    ServerConcealSession.Terminate();
                    ServerConcealSession = null;
                    break;
                case RunLocation.Singleplayer:
                    ServerConcealSession.Terminate();
                    ServerConcealSession = null;
                    ClientConcealSession.Terminate();
                    ClientConcealSession = null;
                    break;
            }

            Log.Trace("Finished Terminating Conceal Sessions", "Initialize");
        }

    }

}
