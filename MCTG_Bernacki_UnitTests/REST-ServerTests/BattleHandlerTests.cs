using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MCTG_Bernacki;
using System.Linq;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    public class BattleHandlerTests
    {
        [Test]
        public void Battle_DeterminesWinnerPlayerTwo_ReturnsInt()
        {
            //Arrange
            FireElve c1 = new FireElve();
            Dragon c2 = new Dragon();
            Orc c3 = new Orc();
            Wizzard c4 = new Wizzard();
            FireSpell c5 = new FireSpell();
            WaterSpell c6 = new WaterSpell();
            Goblin c7 = new Goblin();

            List<Card> deckOne = new List<Card>();
            List<Card> deckTwo = new List<Card>();

            deckOne.Add(c1);deckOne.Add(c2);deckOne.Add(c3);deckOne.Add(c4);deckOne.Add(c5);
            deckTwo.Add(c2); deckTwo.Add(c3); deckTwo.Add(c4); deckTwo.Add(c5); deckTwo.Add(c6);


            BattleHandler game = new BattleHandler(deckOne, deckTwo);

            // Act
            int winner = game.Battle();

            // Assert
            Assert.AreEqual(2, winner);
        }


        [Test]
        public void Battle_DeterminesWinnerDraws_ReturnsInt()
        {
            //Arrange
            FireElve c1 = new FireElve();
            Dragon c2 = new Dragon();
            Orc c3 = new Orc();
            Wizzard c4 = new Wizzard();
            FireSpell c5 = new FireSpell();
            WaterSpell c6 = new WaterSpell();
            Goblin c7 = new Goblin();

            List<Card> deckOne = new List<Card>();
            List<Card> deckTwo = new List<Card>();

            deckOne.Add(c1); deckOne.Add(c2); deckOne.Add(c3); deckOne.Add(c4); deckOne.Add(c5);
            deckTwo.Add(c1); deckTwo.Add(c2); deckTwo.Add(c3); deckTwo.Add(c4); deckTwo.Add(c5);


            BattleHandler game = new BattleHandler(deckOne, deckTwo);

            // Act
            int winner = game.Battle();

            // Assert
            Assert.AreEqual(0, winner);
        }

        [Test]
        public void Battle_DeterminesWinnerOne_ReturnsInt()
        {
            //Arrange
            FireElve c1 = new FireElve();
            Dragon c2 = new Dragon();
            Orc c3 = new Orc();
            Wizzard c4 = new Wizzard();
            FireSpell c5 = new FireSpell();
            WaterSpell c6 = new WaterSpell();
            Goblin c7 = new Goblin();
            NormalSpell c8 = new NormalSpell();

            List<Card> deckOne = new List<Card>();
            List<Card> deckTwo = new List<Card>();

            deckOne.Add(c7); deckOne.Add(c3); deckOne.Add(c8); deckOne.Add(c5); deckOne.Add(c6);
            deckTwo.Add(c1); deckTwo.Add(c2); deckTwo.Add(c3); deckTwo.Add(c4); deckTwo.Add(c5);


            BattleHandler game = new BattleHandler(deckOne, deckTwo);

            // Act
            int winner = game.Battle();

            // Assert
            Assert.AreEqual(1, winner);
        }

        [Test]
        public void Battle_ReturnLogsBattleOne_ReturnsString()
        {
            //Arrange
            FireElve c1 = new FireElve();
            Dragon c2 = new Dragon();
            Orc c3 = new Orc();
            Wizzard c4 = new Wizzard();
            FireSpell c5 = new FireSpell();
            WaterSpell c6 = new WaterSpell();
            Goblin c7 = new Goblin();
            NormalSpell c8 = new NormalSpell();

            List<Card> deckOne = new List<Card>();
            List<Card> deckTwo = new List<Card>();

            deckOne.Add(c7); deckOne.Add(c3); deckOne.Add(c8); deckOne.Add(c5); deckOne.Add(c6);
            deckTwo.Add(c1); deckTwo.Add(c2); deckTwo.Add(c3); deckTwo.Add(c4); deckTwo.Add(c5);


            BattleHandler game = new BattleHandler(deckOne, deckTwo);
            int winner = game.Battle(); // Player One wins here!

            // Act
            List<String> logEntrys = game.ReturnLogs();
            String lastLog = logEntrys.Last();

            // Assert
            Assert.IsNotNull(lastLog);
        }



    }
}
