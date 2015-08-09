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
using Commands = SEGarden.Chat.Commands;
using SEGarden.Notifications;

namespace GardenPerformance.Components {

    /// <summary>
    /// LoadData, UnloadData, Update Before/After/Simulate, UpdatingStopped
    /// </summary>
	[Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.BeforeSimulation)]
	class Session : Sandbox.Common.MySessionComponentBase {

        private static SEGarden.Logging.Logger Logger;
        private static Commands.Processor CommandProcesor;

        private int Frame;
        private AlertNotification testNotice;
        //System.IO.TextWriter TextWriter;
        //SEGarden.Files.TextHandler TextFileHandler;

        public override void LoadData() {
            base.LoadData();

            Logger = new SEGarden.Logging.Logger("GardenPerformance.Components");
            /*
            Logger.info("Starting", "Init");
             * */

            CommandProcesor = new Commands.Processor();
            CommandProcesor.LoadData();


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
            CommandProcesor.UnloadData();

        }

        /*
		public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {
			base.Init(sessionComponent);
		}
         * */

		public override void UpdateBeforeSimulation() {
			base.UpdateBeforeSimulation();

            AlertNotification testNotice1;

            Frame++;

            if (Frame % 100 == 0) {

                /*
                if (TextWriter == null) {
                    TextWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage("pooptallk.txt", typeof(AlertNotification));
                }

                if (TextFileHandler == null) {
                    TextFileHandler = new SEGarden.Files.TextHandler("ponaniponani.txt");
                }
                 *                  * */


                try {
                    //Logger.info("Update frame " + Frame, "Init");
                    //TextWriter.WriteLine("POOOOOOOP");
                    //TextWriter.Flush();
                    //TextFileHandler.WriteLine("pooooop");

                    Logger.Info("jelly", "updatePoop");

                    testNotice1 = new AlertNotification() {
                        Text = "Successfully logged"
                    };
                    testNotice1.Raise();


                }
                catch (Exception e) {
                    testNotice1 = new AlertNotification() {
                        Text = "Exception: " + e.ToString()
                    };
                    testNotice1.Raise();
                }

                /*
                AlertNotification testNotice1 = new AlertNotification() {
                    Text = "CommandProcessor null? " + (CommandProcesor == null).ToString()
                };

                AlertNotification testNotice2 = new AlertNotification() {
                    Text = "Testing, testing, 1 2 3 - " + Frame + " test notice null? " + (testNotice == null).ToString()
                };
                AlertNotification testNotice3 = new AlertNotification() {
                    Text = "Logger null? " + (Logger == null).ToString()
                };

                testNotice1.Raise();

                testNotice2.Raise();

                testNotice3.Raise();
                 * */
            }


			//if (m_CoreProcessor == null)
			//	startCore();

			//m_CoreProcessor.updateBeforeSimulation();
		}



        /*
		/// <summary>
		/// Starts up the proper core process depending on whether we are a client or server.
		/// </summary>
		private void startCore() {
			if (Utility.isServer()) {
				m_CoreProcessor = new Core_Server();
			} else {
				m_CoreProcessor = new Core_Client();
			}

			m_CoreProcessor.initialize();
		}
         * */
	}

}