
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.ObjectBuilders;
using VRage.Library.Utils;
using Interfaces = Sandbox.ModAPI.Interfaces;
using InGame = Sandbox.ModAPI.Ingame;

using SEGarden.Chat;
using Commands = SEGarden.Chat.Commands;
using SEGarden.Notifications;



//using Sandbox.Common.Components;
//using InGame = Sandbox.ModAPI.Ingame;

using VRage.ModAPI;

namespace GardenPerformance.Components {


	public static class Concealment {

        public abstract class ConcealedEntity {
            public long EntityId;
            public VRageMath.Vector3D Position;
            //public EntityType EntityType;
        }

        public class ConcealedGrid : ConcealedEntity {
            public List<SpawnPoint> SpawnPoints;
        }

        public class SpawnPoint {
            public long OwnerId;
        }

        [FlagsAttribute] 
        public enum Concealability : short
        {
            Concealable = 0,
            Controlled = 1,
            NearControlled = 2,
            Moving = 4,
            Working = 8, // refining or manufacturing
            NearAsteroid = 16,
            NeededForSpawn = 32,
        };

        [FlagsAttribute]
        public enum Revealability : short {
            Revealable = 0,
            Blocked = 1,
        };

        public class ConcealmentState {
            public List<ConcealedGrid> ConcealedGrids;
        }

        //public enum EntityType { FloatingObject, Grid, Asteroid, Planet, Character }

        // I would love to store the builder within our ConcealEntitiy representations,
        // but that means we have to pass them for external commands, and we can't
        // serialize them together anyway (error when you make an objectbuilder a field
        // on any new object you try to serialize. They work fine by themselves tho)
        private static Dictionary<long, MyObjectBuilder_EntityBase> ConcealedBuilders
            = new Dictionary<long, MyObjectBuilder_EntityBase>();

        private static Dictionary<long, ConcealedEntity> ConcealedEntities = 
            new Dictionary<long, ConcealedEntity>();

        private static SEGarden.Logging.Logger Logger = 
            new SEGarden.Logging.Logger("GardenPerformance.Components.Concealer");

        #region Lists

        /// <summary>
        /// Create a copy of the Grid info as list, for admin tasks
        /// </summary>
        /// <returns></returns>
        /// Dictionary<long, ConcealedGrid> ConcealedGrids() {
        public static List<ConcealedGrid> ConcealedGrids() {
            return ConcealedEntities.Values.Select(x => x as ConcealedGrid).
                Where(x => x != null).ToList();
            //.Where(kvp => kvp.Value is ConcealedGrid).Values.ToList();
            //ToDictionary(kvp => kvp.Key, kvp => kvp.Value as ConcealedGrid);
        }

        public static List<IMyCubeGrid> RevealedGrids() {
            var gridEntities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(gridEntities, e => e is IMyCubeGrid);
            return gridEntities.Select(x => x as IMyCubeGrid).ToList();
        }

        #endregion
        #region Conceal

        /// <summary>
        /// Conceal request called from outside this class, i.e. a chat command
        /// </summary>
        /// <param name="grid"></param>
        public static bool RequestConceal(long entityId) {
            IMyEntity entity = MyAPIGateway.Entities.GetEntityById(entityId);
            
            // TODO: check concealability and pass some sort of info back?
            // or should we just do it since the user is asking?
            QueueConceal(entity);
            return true;
        }

        private static Concealability GetConcealability(IMyEntity entity) {
            // TODO: check for concealment conditionals
            return Concealability.Concealable;
        }


        private static void QueueConceal(IMyEntity entity) {
            // TODO: wait to conceal after notifying other mods for a few frames
            ConcealEntity(entity);
        }


        private static void ConcealEntity(IMyEntity entity) {
            if (entity == null) {
                Logger.Error("Received null entity, aborting", "ConcealEntity");
                return;
            }

            if (entity.SyncObject == null) {
                Logger.Error("SyncObject missing, aborting", "ConcealEntity");
                return;
            }

            MyObjectBuilder_EntityBase builder = entity.GetObjectBuilder();

            if (builder == null) {
                Logger.Error("Unable to retrieve builder for " + entity.EntityId +
                    ", aborting", "ConcealEntity");
                return;
            }

            // Track it

            if (ConcealedBuilders.ContainsKey(builder.EntityId)) {
                Logger.Error("Attempting to store already-stored entity " + 
                    entity.EntityId, "ConcealEntity");
                return;
            }

            ConcealedBuilders.Add(builder.EntityId, builder);

            if (builder is MyObjectBuilder_CubeGrid) {
                ConcealedEntities.Add(builder.EntityId,
                    new ConcealedGrid() {
                        EntityId = builder.EntityId,
                        SpawnPoints = new List<SpawnPoint>(), // TODO: Check for spawn points
                        Position = new VRageMath.Vector3D(), // TODO: Get position
                    }
                );
            }
            else {
                ConcealedEntities.Add(builder.EntityId,
                    new ConcealedGrid() {
                        EntityId = builder.EntityId,
                        Position = new VRageMath.Vector3D(), // TODO: Get position
                    }
                );
            }

            QueueSave();

            // Remove it from the world
            entity.SyncObject.SendCloseRequest();
        }

        #endregion
        #region Reveal

        /// <summary>
        /// Conceal request called from outside this class, i.e. a chat command
        /// </summary>
        /// <param name="grid"></param>
        public static bool RequestReveal(long entityId) {
            if (!ConcealedEntities.ContainsKey(entityId)) {
                // TODO: notify requestor somehow
                return false;
            }

            ConcealedEntity entity = ConcealedEntities[entityId];

            // TODO: check revealability and pass some sort of info back?
            // or should we just do it since the user is asking?
            QueueReveal(entity);
            return true;
        }

        private static Revealability GetRevealability(ConcealedEntity entity) {
            // TODO: check for concealment conditionals
            return Revealability.Revealable;
        }


        /// <summary>
        /// TODO: Actually queue, just like conceal
        /// </summary>
        /// <param name="entity"></param>
        public static void QueueReveal(ConcealedEntity entity) {
            revealEntity(entity);
        }


        private static void revealEntity(ConcealedEntity entity) {
            Logger.Trace("Start reveal " + entity.EntityId, "revealEntity");

            if (entity == null) {
                Logger.Error("Received null entity, aborting", "RevealEntity");
                return;
            }

            MyObjectBuilder_EntityBase builder = ConcealedBuilders[entity.EntityId];

            if (builder == null) {
                Logger.Error("Unable to retrieve builder for " + entity.EntityId +
                    ", aborting", "RevealEntity");
                return;
            }

            // Remove from trackers

            if (entity is ConcealedGrid) {
                ConcealedGrid grid = entity as ConcealedGrid;
                MyObjectBuilder_CubeGrid gridBuilder = 
                    builder as MyObjectBuilder_CubeGrid;

                if (builder == null) {
                    Logger.Error("Wrong builder type stored for " + entity.EntityId +
                        ", aborting", "RevealEntity");
                    return;
                }

                ConcealedEntities.Remove(entity.EntityId);
                ConcealedBuilders.Remove(entity.EntityId);

                if (MyAPIGateway.Entities.EntityExists(builder.EntityId)) {
                    gridBuilder.EntityId = 0;
                    Logger.Trace("Reallocating entityId", "revealEntity");
                }

                //builder.LinearVelocity = VRageMath.Vector3D.Zero;
                //builder.AngularVelocity = VRageMath.Vector3D.Zero;

                MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(gridBuilder);

                Logger.Trace("Created object", "revealEntity");
            }

            //Reveal

            Logger.Trace("End reveal " + entity.EntityId, "revealEntity");
        }

        #endregion
        #region Saving

        private static void QueueSave() {
            // TODO: Set a flag to do this during update instead to multiple
            // Want to wait a while too, this flag gets hit every conceal
            Save();
        }

        private static void Save() {
            Logger.Trace("Saving", "Save");

            // TODO: Get the world name
            String worldName = "TestWorld";

            // TODO: Store more types of entities

            ConcealmentState state = new ConcealmentState() {
                ConcealedGrids = ConcealedGrids()
            };

            SEGarden.Files.Manager.writeLine(
                MyAPIGateway.Utilities.
                SerializeToXML<ConcealmentState>(state),
                worldName + "_concealment_state.txt"
            );

            // can't store object builders as a member of another object,
            // until we can store object builders in a higher level object,
            // serializing all the different types will require a few different steps
            // and unfortunately a few different files.

            //List<MyObjectBuilder_EntityBase> concealedBuilderList =
            //    ConcealedBuilders.Values.ToList();

            List<MyObjectBuilder_CubeGrid> concealedGridBuilderList =
                ConcealedBuilders.Values.Select(x => x as MyObjectBuilder_CubeGrid).
                Where(x => x != null).ToList();

            SEGarden.Files.Manager.writeLine(
                MyAPIGateway.Utilities.
                SerializeToXML<List<MyObjectBuilder_CubeGrid>>(concealedGridBuilderList),
                worldName + "_concealed_grid_builders.txt"
            );

            Logger.Trace("Finished saving", "Save");
        }

        #endregion
        #region Loading

        #endregion

    }
}
