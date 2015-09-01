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
using SEGarden.Math;

using VRageMath;

namespace GP.Concealment.World.Entities {

    public class ConcealedGrid : ConcealedEntity, ConcealableGrid {

        #region Static

        #endregion
        #region Fields

        #endregion
        #region Properties

        public List<long> SpawnOwners { get; set; }
        public List<long> BigOwners { get; set; }
        public bool IsInsideAsteroid { get; set; }

        [XmlIgnore]
        public IMyCubeGrid Grid { get; private set; }
        [XmlIgnore]
        public override bool NeedsReveal {
            get {
                return base.NeedsReveal || NeedsRevealForSpawn;
            }
        }
        [XmlIgnore]
        public bool NeedsRevealForSpawn { get; private set; }

        #endregion
        #region Constructors

        // XML Deserialization
        public ConcealedGrid() : base() {
            TypeOfEntity = EntityType.Grid;
            Log.ClassName = "GP.Concealment.World.Entities.ConcealedGrid";
        }

        // Byte Deserialization
        public ConcealedGrid(VRage.ByteStream stream) : base(stream) {
            SpawnOwners = stream.getLongList();
            BigOwners = stream.getLongList();
        }

        // Creation from ingame entity before it's removed
        public ConcealedGrid(ConcealableGrid grid) : base(grid.Grid as IMyEntity) {
            Grid = grid.Grid;
            BigOwners = grid.BigOwners;
            SpawnOwners = grid.SpawnOwners;
        }

        #endregion
        #region Serialization

        // Byte Serialization
        public void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
            stream.addLongList(SpawnOwners);
            stream.addLongList(BigOwners);
        }

        #endregion
        #region Update Attributes from Ingame data

        /// <summary>
        /// Should be called when the session player list changes
        /// </summary>
        private void UpdateNeedsRevealForSpawn() {
            List<long> toRevealFor = Sessions.ServerConcealSession.Instance.
                Manager.Revealed.ActivePlayersAndAllies;

            foreach (long owner in SpawnOwners) {
                if (toRevealFor.Contains(owner)) {
                    NeedsRevealForSpawn = true;
                    return;
                }
            }

            NeedsRevealForSpawn = false;
        }

        #endregion

        protected override void Reveal() {
            /*
            Log.Error("Revealing entity", "RevealEntity");

            ConcealedGrid concealableGrid;

            // === Get stored concealed grid

            if (Grids.ContainsKey(entityId)) {
                concealableGrid = Grids[entityId];
            }

            if (concealableGrid == null) {
                Log.Error("Failed to find grid, aborting", "ConcealEntity");
                return false;
            }

            // === Get stored builder

            MyObjectBuilder_CubeGrid builder = null;

            if (GridBuilders.ContainsKey(entityId)) {
                builder = GridBuilders[entityId];
            }

            if (builder == null) {
                Log.Error("Unable to retrieve builder for " + concealableGrid.EntityId +
                    ", aborting", "RevealEntity");
                return false;
            }

            // Reallocate ID if necessary
            if (MyAPIGateway.Entities.EntityExists(concealableGrid.EntityId)) {
                concealableGrid.EntityId = 0;
                Log.Trace("Reallocating entityId", "revealEntity");
            }

            // === Add it back to game world
            Log.Trace("Adding entity back into game from builder. " +
                concealableGrid.EntityId, "revealEntity");
            MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(builder);
            Log.Trace("Created object", "revealEntity");

            // === Update lists
            Grids.Remove(concealableGrid.EntityId);
            GridBuilders.Remove(concealableGrid.EntityId);

            Log.Trace("End reveal " + concealableGrid.EntityId, "revealEntity");
            return true;
            */
        }
    }
}
