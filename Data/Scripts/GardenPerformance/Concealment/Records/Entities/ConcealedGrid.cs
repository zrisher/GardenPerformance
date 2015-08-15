using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Records.Entities {

    public class ConcealedGrid : ConcealedEntity {

        public class SpawnPoint {
            public long OwnerId;
        }

        public List<SpawnPoint> SpawnPoints;
        public Revealability LastKnownRevealability;

        public override Revealability GetRevealability() {
            // TODO: check for concealment conditionals
            return Revealability.Revealable;
        }

        // I can't imagine it being more efficient to actually check the size of the
        // list for this, but maybe we should allocate more? Like 7 longs?
        private int SizeInBytes = sizeof(ushort) *2;


        public byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SizeInBytes, true);

            // TODO: get serialized base entity
            //stream.addLong(EntityId);

            stream.addUShort((ushort)LastKnownRevealability);

            // TODO: spawn points
            //List<long> spawnPointOwners = SpawnPoints.Select(x => x.OwnerId).ToList();
            //stream.addLongList(spawnPointOwners);

            //return stream.Data;
            return new byte[0];

        }

        public static ConcealedGrid FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            ConcealedGrid grid = new ConcealedGrid();

            // TODO: deserialize base entity

            grid.LastKnownRevealability = (Revealability)stream.getUShort();

            // TODO: spawn points
            //List<long> spawnPointOwners = stream.getLongList();
            //foreach (long ownerId in spawnPointOwners)

            return grid;
        }
    }
}
