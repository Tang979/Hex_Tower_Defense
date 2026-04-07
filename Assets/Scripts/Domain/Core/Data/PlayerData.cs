using System.Collections.Generic;

namespace Domain.Core.Data
{
    public class PlayerData
    {
        public string PlayerName;
        public int Coins;

        public List<string> UnlockedTowerIds = new List<string>();
        public List<string> SelectedTowerIds =  new List<string>(5);
    }
}