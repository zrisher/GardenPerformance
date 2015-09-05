using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine.Utils;
using Sandbox.ModAPI;
using VRage.Library.Utils;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using Interfaces = Sandbox.ModAPI.Interfaces;
using InGame = Sandbox.ModAPI.Ingame;

using VRageMath;

using SEGarden;
using SEGarden.Chat;
using SEGarden.Commons.Conceal;
using SEGarden.Logging;
using Commands = SEGarden.Chat.Commands;
using SEGarden.Notifications;
using SEGarden.Extensions;

using SEGarden.Logging;
using SEGarden.Math;

using GP.Concealment.World.Entities;
using GP.Concealment.World.Sectors;

namespace GP.Concealment {

    ///<summary>
    /// Runs as part of the server session
    /// Manages the updating, saving, and processing on conceal and reveal
    /// Holds the info on each respective part in its Concealed and Revealed sectors
    ///</summary>
    public class ConcealmentManager {

        #region Static

        private static Logger Log = new Logger("GP.Concealment.ConcealmentManager");

        #endregion
        #region Instance Fields

        private String WorldName;

        // eventually multiple sectors
        private Vector3D SectorPosition; 
        public ConcealedSector Concealed;
        public RevealedSector Revealed;

        private Queue<ConcealedGrid> GridRevealQueue = new Queue<ConcealedGrid>();
        private Queue<RevealedGrid> GridConcealQueue = new Queue<RevealedGrid>();

        private bool SaveNextUpdate;

        #endregion
        #region Instance Properties

        public bool Loaded { get; private set; }

        #endregion
        #region Constructor

        public ConcealmentManager() {}

        #endregion
        #region Initialize/Terminate

        public void Initialize() {
            Log.Trace("Initializing ConcealmentManager", "Initialize");
            WorldName = MyAPIGateway.Session.Name;
            SectorPosition = MyAPIGateway.Session.GetWorld().Sector.Position;
            Concealed = ConcealedSector.LoadOrNew(WorldName, SectorPosition);

            Revealed = new RevealedSector();
            ControllableEntity.ControllableEntityAdded += Revealed.ControllableEntityAdded;
            ControllableEntity.ControllableEntityMoved += Revealed.ControllableEntityMoved;
            ControllableEntity.ControllableEntityRemoved += Revealed.ControllableEntityRemoved;
            //ControllableEntity.ControlAcquired += Revealed.ControllableEntityControlled;
            //ControllableEntity.ControlReleased += Revealed.ControllableEntityReleased;

            if (Concealed != null) Loaded = true;
            Log.Trace("Done Initializing ConcealmentManager", "Initialize");
        }

        public void Terminate() {
            Log.Trace("Terminating ConcealmentManager", "Terminate");
            if (Loaded) Concealed.Save();

            ControllableEntity.ControllableEntityAdded -= Revealed.ControllableEntityAdded;
            ControllableEntity.ControllableEntityMoved -= Revealed.ControllableEntityMoved;
            ControllableEntity.ControllableEntityRemoved -= Revealed.ControllableEntityRemoved;
            //ControllableEntity.ControlAcquired -= Revealed.ControllableEntityControlled;
            //ControllableEntity.ControlReleased -= Revealed.ControllableEntityReleased;

            Log.Trace("Done Terminating ConcealmentManager", "Terminate");
        }

        #endregion
        #region Conceal/Reveal requests


        // can occur from messaging and entity hooks so safed
        // provide instruction queue for managed resource updates 
        // wait to conceal after notifying other mods for a few frames
        public bool QueueConceal(long entityId) {
            RevealedGrid grid = Revealed.GetGrid(entityId);
            if (grid == null) {
                Log.Error("Couldn't find grid to conceal " + entityId, "QueueConceal");
                return false;
            }

            return QueueConceal(grid);
        }


        public bool QueueConceal(RevealedGrid grid) {
            if (!grid.IsConcealable) {
                Log.Warning("Is not concealable: " + grid.EntityId, "QueueConceal");
                return false;
            }

            Log.Trace("Equeuing grid for conceal " + grid.EntityId, "QueueConceal");
            GridConcealQueue.Enqueue(grid);
            ConcealQueuedMessage msg = new ConcealQueuedMessage(grid.EntityId);
            msg.SendToAll();

            return true;
        }

        public bool QueueReveal(long entityId) {
            ConcealedGrid grid = Concealed.GetGrid(entityId);
            if (grid == null) {
                Log.Error("Couldn't find grid to reveal " + entityId, "QueueReveal");
                return false;
            }

            return QueueReveal(grid);
        }

        public bool QueueReveal(ConcealedGrid grid) {
            if (!grid.IsRevealable) {
                Log.Warning("Is not revealable: " + grid.EntityId, "QueueReveal");
                return false;
            }

            Log.Trace("Equeuing grid for reveal " + grid.EntityId, "QueueReveal");
            GridRevealQueue.Enqueue(grid);
            return true;
        }

        private void QueueSave() {
            SaveNextUpdate = true;
        }

        #endregion
        #region Conceal/Reveal Queue Processing

        public void ProcessConcealQueue() {
            if (GridConcealQueue.Count == 0) return; 

            Log.Trace("Processing Conceal Queue", "ProcessConcealQueue");
            RevealedGrid grid;

            for (ushort i = 0; i < GridConcealQueue.Count; ++i) {
                grid = GridConcealQueue.Dequeue();

                if (grid.TryConceal()) {
                    return; // only one per update, entity addition is expensive
                }
                else {
                    GridConcealQueue.Enqueue(grid);
                }
            }
        }

        public void ProcessRevealQueue() {
            if (GridRevealQueue.Count == 0) return; 

            Log.Trace("Processing Reveal Queue", "ProcessRevealQueue");
            ConcealedGrid grid; 

            for (ushort i = 0; i < GridRevealQueue.Count; ++i) {
                grid = GridRevealQueue.Dequeue();

                if (grid.TryReveal()) {
                    return; // only one per update, entity serialization is expensive
                }
                else {
                    GridRevealQueue.Enqueue(grid);
                }
            }
        }

        #endregion
        #region Save

        public void Save() {
            if (Concealed == null) {
                Log.Error("Failed to save concealed sector, not loaded.", "Save");
                return;
            }

            Concealed.Save();
        }

        #endregion

    }

}
