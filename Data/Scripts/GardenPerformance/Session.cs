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

namespace GardenPerformance {

    /// <summary>
    /// LoadData, UnloadData, Update Before/After/Simulate, UpdatingStopped
    /// </summary>
	[Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.BeforeSimulation)]
    class Session : GardenSession {

        private static Logger Logger = new Logger("GardenPerformance.Components");

        protected override void Initialize() {
            base.Initialize();

            GardenGateway.Commands.addCommands(Commands.FullTree);
            Concealer.Initialize();
        }

        protected override void Terminate() {
            base.Terminate();

            Concealer.Terminate();
        }

    }

}
