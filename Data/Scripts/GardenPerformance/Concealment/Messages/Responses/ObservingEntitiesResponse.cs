using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Logging;
using SEGarden.Extensions;

using GP.Concealment.World.Entities;

namespace GP.Concealment.Messages.Responses {
    class ObservingEntitiesResponse : Response {

        private static Logger Log =
            new Logger("GP.Concealment.Messaging.Messages.Responses.ObservingEntitiesResponse");

        public static ObservingEntitiesResponse FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            ObservingEntitiesResponse response = new ObservingEntitiesResponse();
            response.LoadFromByteStream(stream);

            ushort count = stream.getUShort();
            for (int i = 0; i < count; i++) {
                EntityType entityType = (EntityType)stream.getUShort();

                switch (entityType) {
                    case EntityType.Character:
                        response.ObservingEntities.Add(new Character(stream));
                        break;
                    case EntityType.Grid:
                        response.ObservingEntities.Add(new RevealedGrid(stream));
                        break;
                }
            }

            return response;
        }

        public List<ObservingEntity> ObservingEntities = new List<ObservingEntity>();

        public ObservingEntitiesResponse() :
            base((ushort)MessageType.ObservingEntitiesResponse) { }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(32, true);
            base.AddToByteStream(stream);

            stream.addUShort((ushort)ObservingEntities.Count);
            foreach (ObservingEntity e in ObservingEntities) {
                stream.addUShort((ushort)e.TypeOfEntity);
                e.AddToByteStream(stream);
            }

            return stream.Data;
        }

    }
}
