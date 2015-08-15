//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using SEGarden.Logic.Common;
using SEGarden.Logging;

using GP.Concealment.Sessions;

namespace GP.Concealment {

    /// <summary>
    /// The main session for Concealment.
    /// Initializes, updates, and terminates the server and client sessions,
    /// depending on where we're running.
    /// 
    /// Managed by the main GP Session.
    /// 
    /// Everything but the Client/Server fields should be unneccesary once we're 
    /// using UpdateManager
    /// </summary>
    static class Session {

        private static Logger Log = new Logger("GP.Concealment.Session");
        private static RunLocation RunningOn;

        // These two fields are the entire reason this class exists:
        public static ClientConcealSession Client;
        public static ServerConcealSession Server;

        /// <summary>
        /// Should only be called by GP.Session
        /// Starts the client or server sessions depending on run location
        /// </summary>
        /// <param name="runningOn"></param>
        public static void Initialize(RunLocation runningOn) {
            Log.Trace("Starting Concealment Sessions", "Initialize");

            RunningOn = runningOn;

            switch (RunningOn) {
                case RunLocation.Client:
                    Client = new ClientConcealSession();
                    Client.Initialize();
                    break;
                case RunLocation.Server:
                    Server = new ServerConcealSession();
                    Server.Initialize();
                    break;
                case RunLocation.Singleplayer:
                    Server = new ServerConcealSession();
                    Server.Initialize();
                    Client = new ClientConcealSession();
                    Client.Initialize();
                    break;
            }

            Log.Trace("Finished Starting Concealment Sessions", "Initialize");
        }

        /// <summary>
        /// Should only be called by GP.Session
        /// Stops the client or server sessions depending on run location
        /// </summary>
        public static void Terminate() {
            Log.Trace("Terminating Concealment Sessions", "Initialize");

            switch (RunningOn) {
                case RunLocation.Client:
                    Client.Terminate();
                    Client = null;
                    break;
                case RunLocation.Server:
                    Server.Terminate();
                    Server = null;
                    break;
                case RunLocation.Singleplayer:
                    Server.Terminate();
                    Server = null;
                    Client.Terminate();
                    Client = null;
                    break;
            }

            Log.Trace("Finished Terminating Concealment Sessions", "Initialize");
        }


    }

}
