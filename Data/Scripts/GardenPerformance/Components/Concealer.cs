using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


	static class Concealer {


        private class ConcealedEntity {
            public long EntityId;
            public VRageMath.Vector3D Position;
            public MyObjectBuilder_CubeGrid Builder;
        }

        private class ConcealedGrid : ConcealedEntity {
            public long OwningFleet;
            public bool HasSpawnPoint;
        }

        private static List<ConcealedGrid> concealedGrids = new List<ConcealedGrid>();
        //private static Dictionary<CachedGridInfo, MyObjectBuilder_CubeGrid> cachedGrids = new Dictionary<long, MyObjectBuilder_CubeGrid>();
        //private static Dictionary<long, CachedGridInfo> cachedGridInfo = new Dictionary<long, CachedGridInfo>();
        //cachedGridNextId = 1;

        /*
		private static Logger s_Logger;

		private static void log(String message, String method = null, Logger.severity level = Logger.severity.DEBUG) {
			if (s_Logger == null)
                s_Logger = new Logger("Conquest.Core", "Concealer");

			s_Logger.log(level, method, message);
		}
        */

        private static void concealGrid(IMyCubeGrid grid) {
            //var grid = MyAPIGateway.Entities.GetEntityByName("") as IMyCubeGrid;
            var builder = grid.GetObjectBuilder() as MyObjectBuilder_CubeGrid;

            concealedGrids.Add(new ConcealedGrid() {
                EntityId = builder.EntityId,
                OwningFleet = 0,
                HasSpawnPoint = false,
                Position = new VRageMath.Vector3D(),
                Builder = builder,
            });

            grid.SyncObject.SendCloseRequest();
        }


        private static void revealEntity(VRage.ObjectBuilders.MyObjectBuilder_EntityBase builder) {
            if (MyAPIGateway.Entities.EntityExists(builder.EntityId)) {
                builder.EntityId = 0;
                // this will allow the entity to allocate a new one on Init
            }

            //builder.LinearVelocity = VRageMath.Vector3D.Zero;
            //builder.AngularVelocity = VRageMath.Vector3D.Zero;

            MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(builder);
        }


	}
}
