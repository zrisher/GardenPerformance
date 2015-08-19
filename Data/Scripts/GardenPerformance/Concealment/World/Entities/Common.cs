using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GP.Concealment.World.Entities {

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
        Concealing,
        Revealed,
        Revealing,
    }


    public enum EntityRevealability : ushort {
        Unknown,
        Revealable,
        Blocked,
    }

    [FlagsAttribute]
    public enum EntityConcealability : ushort {
        Unknown,
        Concealable,
        Controlled,
        Visible,
        Detectable,
        Communicative,
        Moving,
        Working,
        NearAsteroid,
        SpawnPoint,
    }

}
