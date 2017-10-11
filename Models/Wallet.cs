using System;

namespace HappyTokenApi.Models
{
    public class Wallet 
    {
        public int Gold { get; set; }

        public int HappyTokens { get; set; }

        public int Gems { get; set; }

        /// <summary>
        /// Returns true if there is nothing in the wallet. Could be useful when wallet is used as Cost where the item cost nothing (normally use for auto upgrade).
        /// </summary>
        /// <value><c>true</c> if is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty => this.Gold == 0 && this.HappyTokens == 0 && this.Gems == 0;

        public void PayFor(Wallet cost)
        {
            this.Gold -= cost.Gold;

            this.Gems -= cost.Gems;

            this.HappyTokens -= cost.HappyTokens;
        }

        public override string ToString() => (Gold > 0 ? "Gold: " + Gold : "") + (Gems > 0 ? "Gems: " + Gems : "") + (HappyTokens > 0 ? "HappyTokens: " + HappyTokens : "");
    }
}
