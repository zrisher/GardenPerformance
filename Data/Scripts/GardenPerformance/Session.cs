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

using GP;
//using GP.Concealment.Sessions;

namespace GP {
    
    /// <summary>
    /// The main session for GardenPerformance.
    /// Initializes, updates, and terminates all sub sessions.
    /// 
    /// This is started by SE as a Session Logic Component,
    /// but GardenSession ensures its not initialized until SEGarden is.
    /// </summary>
	[Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.BeforeSimulation)]
    class Session : GardenSession {

        private static Logger Log = new Logger("GardenPerformance.Session");

        public static readonly String Version = "0.3.5";

        public Session() : base(RunLocation.Any) { }

        protected override void Initialize() {
            base.Initialize();
            Log.Trace("Initializing Garden Performance v" + Version, "Initialize");
            Concealment.Session.Initialize(RunningOn);
            Log.Trace("Finished Initializing Garden Performance", "Initialize");
        }

        protected override void Terminate() {
            base.Terminate();
            Log.Trace("Terminating Garden Performance", "Initialize");
            Concealment.Session.Terminate();
            Log.Trace("Finished Terminating Garden Performance", "Initialize");

        }

    }

}
