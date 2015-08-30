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

    /*
    public enum ConcealStatus : ushort {
        Unknown,
        Concealed,
        Concealing,
        Revealed,
        Revealing,
    }


    public enum EntityRevealability : ushort {
        Revealable,
        Blocked,
    }
    */

    [FlagsAttribute]
    public enum EntityConcealability : ushort {
        Concealable = 0,
        Controlled = 1,
        NearControlled = 2,
        Visible = 4,
        Detectable = 8,
        Communicative = 16,
        Moving = 32,
        Working = 64,
        NearAsteroid = 128,
        SpawnPoint = 256,
    }

}


