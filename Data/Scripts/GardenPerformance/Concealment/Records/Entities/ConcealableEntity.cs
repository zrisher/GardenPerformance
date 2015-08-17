using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Sandbox.ModAPI;
using VRage.ModAPI;

using SEGarden.Extensions;
using SEGarden.Extensions.VRageMath;

namespace GP.Concealment.Records.Entities {


    public class ConcealableEntity {

        #region Subclasses

        public enum EntityType : ushort {
            Unknown,
            Asteroid,
            Character,
            FloatingObject,
            Grid,
            Planet,
        }

        public enum ConcealStatus : ushort {
            Unknown,
            Concealed,
            Revealed,
        };

        public enum EntityRevealability : ushort {
            Unknown,
            Revealable,
            Blocked,
        };

        [FlagsAttribute]
        public enum EntityConcealability : ushort {
            Unknown,
            Concealable,
            Controlled,
            NearControlled,
            Moving,
            Working,
            NearAsteroid,
            NeededForSpawn,
        };

        #endregion
        #region Static

        //public const int SizeInBytes = sizeof(long) + sizeof(ushort) +
        //    Vector3DExtensions.SizeInBytes + sizeof(bool);

        #endregion
        #region Instance

        public long EntityId = 0;
        public VRageMath.Vector3D Position = new VRageMath.Vector3D();
        public bool Transparent = false;
        public EntityRevealability Revealability = EntityRevealability.Unknown;
        public EntityConcealability Concealability = EntityConcealability.Unknown;

        // All inherited types should be able to determine these
        public EntityType Type = EntityType.Unknown;
        public bool IsStatic = false;

        // Only the concealed sector really knows this
        public ConcealStatus Status = ConcealStatus.Unknown;

        public void AddToByteStream(VRage.ByteStream stream) {
            stream.addLong(EntityId);
            stream.addUShort((ushort)Type);
            stream.addVector3D(Position);
            stream.addBoolean(Transparent);
            stream.addBoolean(IsStatic);
            stream.addUShort((ushort)Revealability);
            stream.addUShort((ushort)Concealability);
            stream.addUShort((ushort)Status);
        }

        public void RemoveFromByteStream(VRage.ByteStream stream) {
            EntityId = stream.getLong();
            Type = (EntityType)stream.getUShort();
            Position = stream.getVector3D();
            Transparent = stream.getBoolean();
            IsStatic = stream.getBoolean();
            Revealability = (EntityRevealability)stream.getUShort();
            Concealability = (EntityConcealability)stream.getUShort();
            Status = (ConcealStatus)stream.getUShort();
        }

        public void LoadFromEntity(IMyEntity entity) {
            EntityId = entity.EntityId;
            Position = entity.GetPosition();
            Transparent = entity.Transparent;
            
            // DO the timer-taker updates later when we have time:
            //RefreshRevealability();
            //RefreshConcealability();

        }

        public void Refresh() {
            //IMyEntity ingameEntity = null;
            //MyAPIGateway.Entities.TryGetEntityById(EntityId, out ingameEntity);


        }

        public void RefreshRevealability() {
            if (Status == ConcealStatus.Revealed) return;

            // TODO: check for concealment conditionals
            Revealability = EntityRevealability.Revealable;
        }

        public void RefreshConcealability() {
            if (Status == ConcealStatus.Concealed) return;

            // TODO: check for revealment conditionals
            Concealability = EntityConcealability.Concealable;
        }


        #endregion

    }
}
