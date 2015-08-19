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
using InGame = Sandbox.ModAPI.Ingame;

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
using GP.Concealment.MessageHandlers;

namespace GP.Concealment.Sessions {

    class ServerConcealSession : SessionComponent {

        public static ServerConcealSession Instance;

        /*
        private static Logger Log =
            new Logger("GardenPerformance.Concealment.Sessions.ServerConcealSession");

        private RunStatus Status = RunStatus.NotInitialized;

        private ServerMessageHandler Messenger;

        // ConcealedSector - load and save
        public ConcealedSector ConcealedSector0;

        public RevealedSector RevealedSector0;
        
        public String Name { get { return "ServerConcealSession"; } }

        // RevealedSector - hook to gameworld, update
        //   revealNeededEntities
        //   concealUnneededEntities

        public override void Initialize() {
            Log.Trace("Initializing Server Conceal Session", "Initialize");

            if (Status == RunStatus.Running) {
                Log.Warning("Duplicate initialization attempt, already initialized.", 
                    "Initialize");
                return;
            }

            // === Load Messenger
            Log.Trace("Registering message handler", "Load");
            Messenger = new ServerMessageHandler();

            // === Load Sector

            String worldName = MyAPIGateway.Session.Name;
            VRageMath.Vector3I sectorPos = MyAPIGateway.Session.GetWorld().Sector.Position;
            String saveFileName = ConcealedSector.GenFileName(worldName, sectorPos);

            if (!GardenGateway.Files.Exists(saveFileName)) {
                Log.Info("No existing save file, starting fresh.", "Load");
                ConcealedSector0 = new ConcealedSector();
                ConcealedSector0.FileName = saveFileName;
                ConcealedSector0.WorldName = worldName;
                ConcealedSector0.SectorPosition = sectorPos;
            }
            else {
                ConcealedSector0 = ConcealedSector.Load(saveFileName);

                if (ConcealedSector0 == null) {
                    Log.Error("Error loading sector! Aborting functionality", "Load");
                    Terminate();
                    return;
                }
            }

            Log.Trace("Registering Revealed Sector", "Load");
            RevealedSector0 = new RevealedSector();
            RevealedSector0.Initialize();

            Status = RunStatus.Running;
            Log.Trace("Finished Initializing Server Conceal Session", "Initialize");
        }


        public override void Terminate() {
            Log.Trace("Terminating Server Conceal Session", "Initialize");

            if (Status == RunStatus.Terminated) return;

            Messenger.Disabled = true;

            if (ConcealedSector0 != null) {
                ConcealedSector0.Save();
            }

            if (RevealedSector0 != null) {
                RevealedSector0.Terminate();
            }

            Status = RunStatus.Terminated;
            Log.Trace("Finished Terminating Server Conceal Session", "Initialize");
        }


        public bool QueueConceal(long entityId) {
            // TODO: wait to conceal after notifying other mods for a few frames
            //ConcealEntity(entity);
            return ConcealedSector0.ConcealEntity(entityId);
        }

        /// <summary>
        /// TODO: Actually queue, just like conceal
        /// </summary>
        /// <param name="entity"></param>
        public bool QueueReveal(long entityId) {
            //revealEntity(entity);
            return ConcealedSector0.RevealEntity(entityId);
        }

        public bool CanConceal(long entityId) {
            ConcealableGrid.EntityConcealability concealability = 0;
            Log.Trace("Concealability: " + concealability, "CanConceal");
            Log.Trace("Concealability as int: " + (int)concealability, "CanConceal");

            Log.Trace("EntityID: " + entityId, "CanConceal");
            IMyEntity entity = MyAPIGateway.Entities.GetEntityById(entityId);

            if (entity is IMyCubeGrid) {
                Log.Trace("Entity is a CubeGrid. Performing Concealability check", "CanConceal");

                // Checking if moving.
                MyPhysicsComponentBase gridPhysics = (entity as IMyCubeGrid).Physics;
                if (gridPhysics.AngularAcceleration.AbsMax() != 0 || gridPhysics.AngularVelocity.AbsMax() != 0 ||
                    gridPhysics.LinearAcceleration.AbsMax() != 0 || gridPhysics.LinearVelocity.AbsMax() != 0) {

                    Log.Trace("AngularAcceleration: " + gridPhysics.AngularAcceleration, "CanConceal");
                    Log.Trace("AngularVelocity: " + gridPhysics.AngularVelocity, "CanConceal");
                    Log.Trace("LinearAcceleration: " + gridPhysics.LinearAcceleration, "CanConceal");
                    Log.Trace("LinearVelocity: " + gridPhysics.LinearVelocity, "CanConceal");

                    Log.Trace("Entity moving", "CanConceal");
                    concealability |= ConcealableEntity.EntityConcealability.Moving;
                    Log.Trace("Concealability as int: " + (int)concealability, "CanConceal");
                }

                // Checking if near asteroid
                BoundingSphereD bounds = new BoundingSphereD(entity.GetPosition(), 1250); // Change number to variable later
                List<IMyEntity> nearbyEntities = new List<IMyEntity>();
                Log.Trace("Getting entities given a bound", "CanConceal");
                nearbyEntities = MyAPIGateway.Entities.GetEntitiesInSphere(ref bounds);
                Log.Trace("Got entities given a bound", "CanConceal");

                foreach (IMyEntity nearbyEntity in nearbyEntities) {
                    if (!(nearbyEntity is IMyCubeBlock)) {
                        if (nearbyEntity.GetObjectBuilder() is MyObjectBuilder_VoxelMap) {
                            Log.Trace("Entity is near asteroid", "CanConceal");
                            BoundingSphere AsteroidHr = nearbyEntity.WorldVolumeHr;
                            BoundingSphere Asteroid = nearbyEntity.WorldVolume;
                            BoundingSphere AsteroidLocal = nearbyEntity.LocalVolume;
                            Log.Trace("Center of Asteroid using WorldVolHr BoundingSphere: " + AsteroidHr.Center, "CanConceal");
                            Log.Trace("Radius of Asteroid using WorldVolHr BoundingSphere: " + AsteroidHr.Radius, "CanConceal");

                            Log.Trace("Center of Asteroid using WorldVol BoundingSphere: " + Asteroid.Center, "CanConceal");
                            Log.Trace("Radius of Asteroid using WorldVol BoundingSphere: " + Asteroid.Radius, "CanConceal");

                            Log.Trace("Center of Asteroid using Local BoundingSphere: " + AsteroidLocal.Center, "CanConceal");
                            Log.Trace("Radius of Asteroid using Local BoundingSphere: " + AsteroidLocal.Radius, "CanConceal");

                            Log.Trace("Center of Asteroid using IMyEntity: " + nearbyEntity.GetPosition(), "CanConceal");
                            concealability |= ConcealableEntity.EntityConcealability.NearAsteroid;
                            Log.Trace("Concealability as int: " + (int)concealability, "CanConceal");
                        }
                    }
                }

                List<IMyPlayer> players = new List<IMyPlayer>();

                // Checking for NearControlled
                // TODO: A GetDistance function between 2 entities
                MyAPIGateway.Multiplayer.Players.GetPlayers(
                    players,
                    (p => Vector3D.Distance(p.Controller.ControlledEntity.Entity.GetPosition(), entity.GetPosition()) < 1250) // TODO: Replace num with variable.
                );
                if (players.Count > 0) {
                    Log.Trace("Entity is near Controlled", "CanConceal");
                    concealability |= ConcealableEntity.EntityConcealability.NearControlled;
                    Log.Trace("Concealability as int: " + (int)concealability, "CanConceal");
                }

                List<IMySlimBlock> blocksToCheck = new List<IMySlimBlock>();
                (entity as IMyCubeGrid).GetBlocks(
                    blocksToCheck,
                    (b => b.FatBlock != null && (b.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_Beacon)
                        || b.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_RadioAntenna)
                        || b.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_Refinery)
                        || b.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_Assembler))
                    )
                );

                Log.Trace("Num blocks: " + blocksToCheck.Count, "CanConceal");

                foreach (IMySlimBlock block in blocksToCheck) {
                    // Checking beacon broadcast range
                    if (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_Beacon)) {
                        Log.Trace("Block is beacon", "CanConceal");
                        InGame.IMyBeacon beaconBlock = (InGame.IMyBeacon)block.FatBlock;
                        if (beaconBlock.Enabled) {
                            players.Clear();
                            MyAPIGateway.Multiplayer.Players.GetPlayers(
                                players,
                                (p => Vector3D.Distance(p.Controller.ControlledEntity.Entity.GetPosition(), beaconBlock.GetPosition()) < beaconBlock.Radius)
                            );
                            if (players.Count > 0) {
                                Log.Trace("Entity is near Controlled(Beacon)", "CanConceal");
                                concealability |= ConcealableEntity.EntityConcealability.NearControlled;//Concealability.PlayerInBroadcastRange;
                                Log.Trace("Concealability as int: " + (int)concealability, "CanConceal");
                            }
                        }
                    }

                    // Checking antenna broadcast range
                    if (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_RadioAntenna)) {
                        Log.Trace("Block is antenna", "CanConceal");
                        InGame.IMyRadioAntenna antennaBlock = (InGame.IMyRadioAntenna)block.FatBlock;
                        if (antennaBlock.Enabled) {
                            players.Clear();
                            MyAPIGateway.Multiplayer.Players.GetPlayers(
                                players,
                                (p => Vector3D.Distance(p.Controller.ControlledEntity.Entity.GetPosition(), antennaBlock.GetPosition()) < antennaBlock.Radius)
                            );
                            if (players.Count > 0) {
                                Log.Trace("Entity is near Controlled(Antenna)", "CanConceal");
                                concealability |= ConcealableEntity.EntityConcealability.NearControlled;
                                Log.Trace("Concealability as int: " + (int)concealability, "CanConceal");
                            }
                        }
                    }

                    // Checking assembler and refinery for production
                    if (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_Assembler)
                        || block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_Refinery)) {
                        Log.Trace("Block is refinery/assembler", "CanConceal");
                        InGame.IMyProductionBlock productionBlock = block.FatBlock as InGame.IMyProductionBlock;
                        if (productionBlock.Enabled && productionBlock.IsProducing) {
                            Log.Trace("Entity is working", "CanConceal");
                            concealability |= ConcealableEntity.EntityConcealability.Working;
                            Log.Trace("Concealability as int: " + (int)concealability, "CanConceal");
                        }
                    }
                }

                Log.Trace("Concealability: " + concealability, "CanConceal");
                Log.Trace("Concealability as int: " + (int)concealability, "CanConceal");

                if (concealability == 0) {
                    Log.Trace("Able to conceal", "CanConceal");
                    concealability |= ConcealableEntity.EntityConcealability.Concealable;
                    return true;
                }
            }

            Log.Trace("Unable to conceal", "CanConceal");
            return false;
        }

        public bool CanReveal(long entityId) {
            return true;
        }

        #region Saving

        public void LoadSector() { }

        public void SaveSector() {
            if (ConcealedSector0 != null) ConcealedSector0.Save();
        }

        private static void QueueSave() {
            // TODO: Set a flag to do this during update instead to multiple
            // Want to wait a while too, this flag gets hit every conceal
            //Save();
        }

        #endregion
        #region Loading

        private static void QueueLoad() {
            // TODO: Set a flag to do this during update instead to multiple
            // Want to wait a while too, this flag gets hit every conceal
            //Save();
        }

        #endregion
         * */
    }

}
