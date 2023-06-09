using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    public enum GMTicketCategory : byte
    {
        Stuck = 1,
        BehaviorHarassment = 2,
        Guild = 3,
        Item = 4,
        Environmental = 5,
        NonQuestCreep = 6,
        QuestNPC = 7,
        Technical = 8,
        AccountBilling = 9,
        Character = 10,
        Max
    };
}
