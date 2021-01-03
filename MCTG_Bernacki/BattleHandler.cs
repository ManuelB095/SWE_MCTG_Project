using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public class BattleHandler
    {
        public List<Card> DeckPlOne;
        public List<Card> DeckPlTwo;
        public const int MAX_ROUNDS = 100; 

        public BattleHandler(List<Card> deckOne, List<Card> deckTwo)
        {
            this.DeckPlOne = deckOne;
            this.DeckPlTwo = deckTwo;
        }

        public bool Battle()
        {
            Console.WriteLine("Works up till here");
            return true;










        }






    }




}
