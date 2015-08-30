using System;
using System.Collections.Generic;
using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Serialization;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Voxels;
using Sandbox.Definitions;
using Sandbox.ModAPI;
//using Interfaces = Sandbox.ModAPI.Interfaces;
//using InGame = Sandbox.ModAPI.Ingame;

using VRage.Components;
using VRage.Library.Utils;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

using SEGarden;
using SEGarden.Logging;
using SEGarden.Logic;
//using SEGarden.Notifications;

using GP.Concealment;
using GP.Concealment.World;
using GP.Concealment.World.Entities;
using GP.Concealment.World.Sectors;
using GP.Concealment.MessageHandlers;

namespace GP.Concealment.Sessions {

    class ServerConcealSession : SessionComponent {

        public static ServerConcealSession Instance;

        private static Logger Log =
            new Logger("GP.Concealment.Sessions.ServerConcealSession");

        public ConcealmentManager Manager;
        private ServerMessageHandler Messenger;
        
        public override String ComponentName { get { return "ServerConcealSession"; } }

        // TODO: increase delay on these after done testing
        public override Dictionary<uint, Action> UpdateActions {
            get {
                return new Dictionary<uint, Action> {
                    {60, Manager.ProcessRevealQueue},
                    {120, Manager.ProcessConcealQueue}
                };
            }
        }

        public override void Initialize() {
            Log.Trace("Initializing Server Conceal Session", "Initialize");

            Log.Trace("Loading settings", "Initialize");
            Settings.Load();

            Log.Trace("Registering message handler", "Initialize");
            Messenger = new ServerMessageHandler();

            Log.Trace("Registering concealment manager", "Initialize");
            Manager = new ConcealmentManager();
            Manager.Initialize();
            if (!Manager.Loaded) {
                Log.Error("Error loading sector, conceal disabled.", "Initialize");
                Messenger.Disabled = true;
            }


            base.Initialize();
            Instance = this;
            Log.Trace("Finished Initializing Server Conceal Session", "Initialize");
        }

        public override void Terminate() {
            Log.Trace("Terminating Server Conceal Session", "Terminate");

            Log.Trace("Terminating concealment manager", "Terminate");
            Manager.Terminate();

            Log.Trace("Terminating server messenger", "Terminate");
            Messenger.Disabled = true;

            base.Terminate();
            Log.Trace("Finished Terminating Server Conceal Session", "Terminate");

        }

    }

}
