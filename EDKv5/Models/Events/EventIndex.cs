using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5
{
    public enum EventIndex : byte
    {
        Track100, Track200, Track400, Track800, Track1500, Hurdle100, Hurdle110,
        Relay4x100, Relay4x400,
        HighJump, PoleVault,
        LongJump, TripleJump,
        ShortPut, Discus, Javelin,

        Butterfly50, Backstroke50, Breaststroke50, Freestyle50, Medley50,
        Butterfly100, Backstroke100, Breaststroke100, Freestyle100, Medley100,
        Butterfly200, Backstroke200, Breaststroke200, Freestyle200, Medley200,
        MedleyRelay4x100, MedleyRelay4x200,

        Custom,

        SportsDayStart = Track100,
        TrackEventStart = Track100,
        TrackEventEnd = Relay4x400,
        FieldEventStart = HighJump,
        FieldEventEnd = Javelin,
        SportsDayEnd = Javelin,
        SwimGalaStart = Butterfly50,
        SwimGalaEnd = MedleyRelay4x200,
    }

}
