using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    enum PlayTimeFlag : uint
    {
        ApproachingPartialPlayTime = 0x1000,
        ApproachingNoPlayTime = 0x2000,
        UnhealthyTime = 0x80000000,
    };
}
