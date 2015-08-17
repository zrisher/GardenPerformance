using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Sandbox.Common.ObjectBuilders;

using Sandbox.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

using SEGarden.Extensions;
using SEGarden.Logging;

namespace GP.Concealment.Records.Entities {

    public class ConcealableGrid : ConcealableEntity {

        /*
        public class SpawnPoint {
            public long OwnerId;
        }
        */

        #region Static

        private static Logger Log =
            new Logger("GP.Concealment.Records.Entities.ConcealableEntity");

        #endregion
        #region Instance


        public List<long> SpawnOwners = new List<long>();
        public String DisplayName = "";
        public List<long> BigOwners = new List<long>();
        public MyObjectBuilder_CubeGrid Builder = new MyObjectBuilder_CubeGrid();

        /*

        */
        [XmlIgnore]
        public IMyCubeGrid IngameGrid;
        
        public ConcealableGrid() {
            Type = EntityType.Grid;

            Log.Trace("Initializing new ConcealedGrid", "ctr");
            Log.Trace("DisplayName == null? " + (DisplayName == null), "ctr");
            Log.Trace("Builder == null? " + (Builder == null), "ctr");

            try {
            }
            catch (Exception e) {

            }
        }

        public void LoadFromCubeGrid(IMyCubeGrid grid) {
            base.LoadFromEntity(grid as IMyEntity);

            IngameGrid = grid;

            /*
            DisplayName = grid.DisplayName;
            // ToDo: get all owners instead of big (for targeting)
            BigOwners = grid.BigOwners;
            // ToDo: get real spawn owners (for spawning)
            SpawnOwners = grid.BigOwners;
            Builder = grid.GetObjectBuilder() as MyObjectBuilder_CubeGrid;

            if (Builder == null) {
                Log.Error("Got null builder for entity " + EntityId, "LoadFromCubeGrid");
                Builder = new MyObjectBuilder_CubeGrid();

            }
            */
        }

        public void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
            /*
            stream.addLongList(SpawnOwners);
            stream.addString(DisplayName);
            stream.addLongList(BigOwners);
            */
        }

        public void RemoveFromByteStream(VRage.ByteStream stream) {
            base.RemoveFromByteStream(stream);
            /*
            SpawnOwners = stream.getLongList();
            DisplayName = stream.getString();
            BigOwners = stream.getLongList();
            */
        }

        #endregion

    }
}
