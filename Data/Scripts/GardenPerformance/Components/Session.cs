
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

using SEGarden.Chat;
using SEGarden.Chat.Commands;
using SEGarden.Notifications;

namespace GardenPerformance.Components {

    /// <summary>
    /// LoadData, UnloadData, Update Before/After/Simulate, UpdatingStopped
    /// </summary>
	[Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.BeforeSimulation)]
	class Session : Sandbox.Common.MySessionComponentBase {

        private static SEGarden.Logging.Logger Logger;
        private static SEGarden.Chat.Commands.Processor CommandProcessor;

        private int Frame;
        private AlertNotification testNotice;
        //System.IO.TextWriter TextWriter;
        //SEGarden.Files.TextHandler TextFileHandler;

        public override void LoadData() {
            base.LoadData();

            Logger = new SEGarden.Logging.Logger("GardenPerformance.Components");
            //Logger.info("Starting", "Init");


            CommandProcessor = new SEGarden.Chat.Commands.Processor();
            CommandProcessor.LoadData();
            CommandProcessor.addCommands(Commands.FullTree);

            testNotice = new AlertNotification() {
                Text = "Testing, testing, 1 2 3"
            };

        }

        protected override void UnloadData() {
            base.UnloadData();

            //TextWriter.Close();
            //TextWriter = null;
            //TextFileHandler.UnloadData();

            //Logger.close();
            SEGarden.Files.Manager.Close();
            CommandProcessor.UnloadData();

        }


		public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {
			base.Init(sessionComponent);
		}


		public override void UpdateBeforeSimulation() {
			base.UpdateBeforeSimulation();

            Frame++;

            if (Frame % 100 == 0) {

                try {


                }
                catch (Exception e) {
                    testNotice = new AlertNotification() {
                        Text = "Exception: " + e.ToString()
                    };
                    testNotice.Raise();
                }

            }

		}


	}

}
