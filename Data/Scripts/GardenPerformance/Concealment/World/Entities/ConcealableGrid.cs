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
using SEGarden.Extensions.Objectbuilders;
using SEGarden.Logging;

namespace GP.Concealment.World.Entities {

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
        //public List<long> BigOwners = new List<long>();

        // Giving this a default value of new MyObjectBuilder_CubeGrid() does NOT work
        // The new object will have null fields, which makes it unsaveable.
        //public MyObjectBuilder_CubeGrid Builder;
        //public MyObjectBuilder_EntityBase Builder;

        [XmlIgnore]
        public IMyCubeGrid IngameGrid;
        
        public ConcealableGrid() {
            Type = EntityType.Grid;
        }

        /*
        public bool Saveable() {

            if (!base.Saveable()) {
                Log.Error("Concealable Entity had a null, not saveable", "Saveable");
                return false;
            }

            if (BigOwners == null || DisplayName == null || Position == null ||  
                SpawnOwners == null) 
            {
                Log.Error("ConcealableGrid had a null, not saveable", "Saveable");
                return false;
            }

            /*
            if (Builder == null) {
                Log.Trace("Null builder for entity " + EntityId + "trying reload", 
                    "Saveable");

                //AttemptReloadFromModAPI();

                if (Builder == null) {
                    Log.Error("Null builder for entity " + EntityId + ", cannot save.",
                        "Saveable");
                    return false;
                }

                //Builder.FillNullsWithDefaults();
            }
            *//*
            return true;
        }
        */

        /*
        private void AttemptReloadFromModAPI() {
            Log.Error("AttemptReloadFromModAPI", "AttemptReloadFromModAPI");

            IMyEntity entity = null;
            MyAPIGateway.Entities.TryGetEntityById(EntityId, out entity);

            if (entity == null) {
                Log.Error("Couldn't find entity via Gateway", "AttemptReloadFromModAPI");
                return;
            }

            IMyCubeGrid grid = entity as IMyCubeGrid;

            if (grid == null) {
                Log.Error("Entity not grid", "AttemptReloadFromModAPI");
                return;
            }

            LoadFromCubeGrid(grid);
            Log.Error("Reloaded.", "AttemptReloadFromModAPI");
        }
        */

        public void LoadFromCubeGrid(IMyCubeGrid grid) {
            base.LoadFromEntity(grid as IMyEntity);

            IngameGrid = grid;

            //IngameGrid = grid;
            DisplayName = grid.DisplayName;
            // ToDo: get all owners instead of big (for targeting)
            //BigOwners = grid.BigOwners;
            // ToDo: get real spawn owners (for spawning)
            SpawnOwners = grid.BigOwners;
            //IMyEntity entity = grid as IMyEntity;
            //Builder = grid.GetObjectBuilder() as MyObjectBuilder_CubeGrid;

            /*
            if (Builder == null) {
                Log.Error("Got null builder for entity " + EntityId +
                    ". We will be unable to save it now.", "LoadFromCubeGrid");
            }
            else {
                //Log.Error("Filling builder nulls for " + EntityId, "LoadFromCubeGrid");
                //Builder.FillNullsWithDefaults();
            }
            */

        }

        public void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
            stream.addLongList(SpawnOwners);
            stream.addString(DisplayName);
            //stream.addLongList(BigOwners);
        }

        public void RemoveFromByteStream(VRage.ByteStream stream) {
            base.RemoveFromByteStream(stream);
            SpawnOwners = stream.getLongList();
            DisplayName = stream.getString();
            //BigOwners = stream.getLongList();
        }

        #endregion

    }
}
