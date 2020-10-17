using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCTG_Bernacki
{
    class Player
    {
        public string Name { get; private set; }
        public List<Card>HandCards { get; private set; }
        public List<Card>DrawPile { get; private set; }

        public Player(string name, List<Card>deck)
        {
            this.Name = name;
            foreach (Card card in deck) {
                this.DrawPile.Append(card);
            }
            this.HandCards = new List<Card>();

        }

        // Shuffle Deck before Game (Kind of a Fisher-Yates Shuffle)
        // Probably better fit in BattleHandler
        public void ShuffleDeck()
        {
            var rand = new Random();
            int n = this.DrawPile.Count;
            while(n>1)
            {
                n--;
                int k = rand.Next(n + 1);
                Card temp = this.DrawPile[k];
                this.DrawPile[k] = this.DrawPile[n];
                this.DrawPile[n] = temp;
            }
        }



    }
}
