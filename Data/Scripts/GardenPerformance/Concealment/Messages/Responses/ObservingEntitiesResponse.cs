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
            long responseLength = bytes.Length;
            Log.Info("Creating new stream of length " + responseLength, "FromBytes");
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            ObservingEntitiesResponse response = new ObservingEntitiesResponse();
            response.LoadFromByteStream(stream);

            Log.Info("Base pos " + stream.Position, "FromBytes");
            ushort count = stream.getUShort();
            Log.Info("With count pos " + stream.Position, "FromBytes");
            for (int i = 0; i < count; i++) {
                Log.Info("Beginning get entity at pos " + stream.Position + " / " + stream.Length, "FromBytes");
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

            Log.Info("Finished getting entity at final pos " + stream.Position, "FromBytes");
            return response;
        }

        public List<ObservingEntity> ObservingEntities = new List<ObservingEntity>();

        public ObservingEntitiesResponse() :
            base((ushort)MessageType.ObservingEntitiesResponse) { }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(32, true);
            base.AddToByteStream(stream);

            Character character;
            RevealedGrid grid;

            Log.Info("Beginning serializing message at position " + stream.Position, "ToBytes");

            stream.addUShort((ushort)ObservingEntities.Count);
            Log.Info("With count pos " + stream.Position, "ToBytes");

            foreach (ObservingEntity e in ObservingEntities) {

                EntityType entityType = e.TypeOfEntity;

                Log.Info("Beginning add entity of type " + entityType + " at pos " + stream.Position + " / " + stream.Length, "ToBytes");

                stream.addUShort((ushort)entityType);

                switch (entityType) {
                    case EntityType.Character:
                        character = e as Character;
                        character.AddToByteStream(stream);
                        break;
                    case EntityType.Grid:
                        grid = e as RevealedGrid;
                        grid.AddToByteStream(stream);
                        break;
                }

                Log.Info("Finished adding entity at final pos " + stream.Position, "ToBytes");
            }

            Log.Info("Finished serializing message at position " + stream.Position, "ToBytes");
            return stream.Data;
        }

    }
}
