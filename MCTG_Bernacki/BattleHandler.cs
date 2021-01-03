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

        public int Battle()
        {
            Console.WriteLine("Works up till here");
            return 1;

            // TO DO:
            // -> Battle Logic ( Rounds, Max Rounds, Damage between Cards etc.
            // -> Determine Winner And/or Round Limit overriden => Draw
            // -> Return:
            // -> -> Winner
            // -> -> Rounds played
            // -> Make Log File of everything, that was going on and save it locally!









        }






    }




}
