﻿using System;
using System.Collections.Generic;
using System.Text;

using Sandbox.Common;

using Sandbox.Common.ObjectBuilders;

//using Sandbox.Definitions;
using Sandbox.ModAPI;
/*
using VRage.Library.Utils;
using Interfaces = Sandbox.ModAPI.Interfaces;
using InGame = Sandbox.ModAPI.Ingame;
*/

using SEGarden.Logic;

using GP.Concealment.Sessions;
using GP.Concealment.World.Entities;

namespace GP {
    
    /// <summary>
    /// The main session for GardenPerformance.
    /// Initializes, updates, and terminates all sub sessions.
    /// 
    /// This is started by SE as a Session Logic Component,
    /// but GardenSession ensures its not initialized until SEGarden is.
    /// </summary>
    [Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.NoUpdate)]
    class Main : MySessionComponentBase {

        public Main() {
            UpdateManager.RegisterSessionComponent(new ServerConcealSession());
            UpdateManager.RegisterSessionComponent(new ClientConcealSession());

            UpdateManager.RegisterEntityComponent(
                ((e) => {
                    Character c = new Character(e);
                    return c;
                }),
                typeof(MyObjectBuilder_Character),
                RunLocation.Server);

            UpdateManager.RegisterEntityComponent(
                ((e) => {
                    RevealedGrid c = new RevealedGrid(e);
                    return c;
                }),
                typeof(MyObjectBuilder_CubeGrid),
                RunLocation.Server);

        }

    }
}
