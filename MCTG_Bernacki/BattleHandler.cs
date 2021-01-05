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
        private bool running;
        private int Rounds { get; set; }
        private List<String> Logs { get; set; }

        public BattleHandler(List<Card> deckOne, List<Card> deckTwo)
        {
            this.DeckPlOne = deckOne;
            this.DeckPlTwo = deckTwo;
            this.running = true;
            this.Rounds = 1;
            this.Logs = new List<String>();
        }

        public int Battle()
        {
            Console.WriteLine("Works up till here");

            while(this.GameEnded() == false)
            {
                // Choose which Cards fight one another:
                Random rndPlOne = new Random();
                int chosenOne = rndPlOne.Next(1, this.DeckPlOne.Count);

                Random rndPlTwo = new Random();
                int chosenTwo = rndPlTwo.Next(1, this.DeckPlTwo.Count);

                this.PlayRound(this.DeckPlOne[chosenOne], this.DeckPlTwo[chosenTwo]);
                if(this.DeckPlOne.Count < 1)
                {
                    String win = "\nPlayer B has defeated Player A after ";
                    win += this.Rounds + " rounds of combat!\n";
                    this.Logs.Add(win);
                    this.EndGame();
                    return 2;
                }
                else if (this.DeckPlTwo.Count < 1)
                {
                    String win = "\nPlayer A has defeated Player B after ";
                    win += this.Rounds + " rounds of combat!\n";
                    this.Logs.Add(win);
                    this.EndGame();
                    return 1;
                }
                this.Rounds += 1;
            }
            return 0; // When MAX_ROUNDS is reached, the result is a Draw!
        }

        private bool GameEnded()
        {
            if(this.running == false || this.Rounds >= BattleHandler.MAX_ROUNDS)
            {
                return true;
            }
            return false;
        }

        private void EndGame()
        {
            this.running = false;
        }

        private void PlayRound(Card plOne, Card plTwo)
        {
            int plOneDamageValue = plOne.CalcDamage(plTwo);
            int plTwoDamageValue = plTwo.CalcDamage(plOne);                      

            String msg = "Player A: ";
            msg += plOne.Name + " (" + plOneDamageValue + " Damage) \n";
            msg += "vs.\n Player B: ";
            msg += plTwo.Name + " (" + plTwoDamageValue + " Damage) \n";

            int winner = this.DetermineWinner(plOneDamageValue, plTwoDamageValue);

            if(plOne is MonsterCard && plTwo is MonsterCard)
            {
                if(winner == 0)
                {
                    msg += "=> " + plOne.Name + " draws " + plTwo.Name + "\n";
                    this.Logs.Add(msg);
                }
                else if (winner == 1)
                {
                    msg += "=> " + plOne.Name + " defeats " + plTwo.Name + "\n";
                    this.Logs.Add(msg);

                }
                else if (winner == 2)
                {
                    msg += "=> " + plTwo.Name + " defeats " + plOne.Name + "\n";
                    this.Logs.Add(msg);

                }
            }            
            else // Spell vs. Spell OR Spell vs. Monster have the same outputs!
            {
                if (winner == 0)
                {
                    msg += "=> " + plOne.Damage + " VS " + plTwo.Damage + " ->" + plOneDamageValue + " VS " + plTwoDamageValue + "\n";
                    msg += "=> " + plOne.Name + " draws " + plTwo.Name + "\n";
                    this.Logs.Add(msg);

                }
                else if (winner == 1)
                {
                    msg += "=> " + plOne.Damage + " VS " + plTwo.Damage + " ->" + plOneDamageValue + " VS " + plTwoDamageValue + "\n";
                    msg += "=> " + plOne.Name + " defeats " + plTwo.Name + "\n";
                    this.Logs.Add(msg);

                }
                else if (winner == 2)
                {
                    msg += "=> " + plOne.Damage + " VS " + plTwo.Damage + " ->" + plOneDamageValue + " VS " + plTwoDamageValue + "\n";
                    msg += "=> " + plTwo.Name + " defeats " + plOne.Name + "\n";
                    this.Logs.Add(msg);

                }
            }

            // Move Cards around, depending on who won the game
            if(winner != 0)
            {                
                if(winner == 1)
                {
                    this.MoveLoserToWinner(plTwo, winner);
                }
                else if(winner == 2)
                {
                    this.MoveLoserToWinner(plOne, winner);
                }
            }


        }

        public List<String> ReturnLogs()
        {
            return this.Logs; 
        }

        private int DetermineWinner(int plOneDamageValue, int plTwoDamageValue)
        {
            if (plOneDamageValue == plTwoDamageValue)
                return 0; // Draw when both equally strong OR both are 0
            else if (plOneDamageValue > plTwoDamageValue)
                return 1; // playerOne`s Card Won 
            else if (plOneDamageValue < plTwoDamageValue)
                return 2; // playerTwo`s Card Won
            else
                return 0; // Draw if no winner can be determined
        }

        private void MoveLoserToWinner(Card loserCard, int winner)
        {
            // int winner = 1 for Player One and = 2 for Player Two;
            if(winner == 1)
            {
                this.DeckPlOne.Add(loserCard);
                this.DeckPlTwo.Remove(loserCard);
            }
            else if(winner == 2)
            {
                this.DeckPlTwo.Add(loserCard);
                this.DeckPlOne.Remove(loserCard);
            }
        }


    }




}
