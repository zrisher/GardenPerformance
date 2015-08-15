using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;
using SEGarden.Extensions.VRageMath;

namespace GP.Concealment.Records.Entities {


    public class ConcealedEntity {

        public long EntityId;
        public EntityType EntityType; // 1 ushort
        public VRageMath.Vector3D Position; // 6 longs

        public virtual Revealability GetRevealability() { return Revealability.Revealable;  }

        private const int SizeInBytes = sizeof(long) * 7 + sizeof(ushort);

        public byte[] ToBytes() {
            /*
            VRage.ByteStream stream = new VRage.ByteStream(SizeInBytes, true);

            stream.addLong(EntityId);
            stream.addUShort((ushort)GetRevealability());

            byte[] vector = Position.ToBytes();
            stream.Write(vector, 0, vector.Length);

            stream.addUShort((ushort)GetRevealability());

            List<long> spawnPointOwners = SpawnPoints.Select(x => x.OwnerId).ToList();
            stream.addLongList(spawnPointOwners);

            //return stream.Data;
             *              * */
            return new byte[0];
        }

        public static ConcealedEntity FromBytes(byte[] bytes) {
            return new ConcealedEntity();
        }
    }
}
