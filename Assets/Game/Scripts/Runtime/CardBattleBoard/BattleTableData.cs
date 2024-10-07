using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Game
{
    [Serializable]
    public class BattleTableData
    {
        public int playersHandCardsAtStart;
        public int playersMaxFuel;
        public int playersMaxTurns;
        public int turnLifeTimeInSeconds;
        public List<SlotData> battlePlayerSlots;
    }
    [Serializable]
    public class SlotData
    {
        public List<string> types;
        public string bgImage;
        
        public NetworkedSlotData ToStruct()
        {
            return new NetworkedSlotData
            {
                types = types.ToArray(),
                bgImage = bgImage
            };
        }
    }
}
