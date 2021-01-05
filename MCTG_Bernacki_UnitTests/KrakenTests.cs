using NUnit.Framework;
using MCTG_Bernacki;
using System.Net.Http.Headers;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    public class KrakenTests
    {
        [Test]
        public void Constructor_Base_ReturnsTrue()
        {
            //Arrange
            Kraken leviathan = new Kraken();

            // Assert
            Assert.IsTrue(leviathan.Name == "Leviathan");
        }

        [Test]
        public void Constructor_WithParameters_ReturnsTrue()
        {
            //Arrange
            Kraken leviathan = new Kraken("KRAKEN", 800, element.WATER, 100);

            //Assert
            Assert.AreEqual(leviathan.Name, "KRAKEN");
        }
        [Test] // Kraken against MonsterCard -> expect Dmg stays the same
        public void CalcDamage_AgainstMonsterCard_ReturnsInt()
        {
            //Arrange
            Kraken leviathan = new Kraken();
            Kraken evenMoreEvilLeviathan = new Kraken();
            //Act
            int leviathanDamage = leviathan.CalcDamage(evenMoreEvilLeviathan);
            //Assert
            Assert.AreEqual(leviathanDamage, leviathan.Damage);
        }

        [Test] // Kraken against FIRE Spell -> expect Dmg stays the same
        public void CalcDamage_AgainstFireSpell_ReturnsInt()
        {
            //Arrange
            Kraken leviathan = new Kraken();
            FireSpell enemySpell = new FireSpell();
            //Act
            int leviathanDamage = leviathan.CalcDamage(enemySpell);
            int expectedDamage = 30;
            //Assert
            Assert.AreEqual(expectedDamage, leviathanDamage);
        }

        [Test] // Kraken against WATER Spell -> expect Dmg stays the same
        public void CalcDamage_AgainstWaterSpell_ReturnsInt()
        {
            //Arrange
            Kraken leviathan = new Kraken();
            WaterSpell enemySpell = new WaterSpell();
            //Act
            int leviathanDamage = leviathan.CalcDamage(enemySpell);
            int expectedDamage = 30;
            //Assert
            Assert.AreEqual(expectedDamage, leviathanDamage);
        }

        [Test] // Kraken against NORMAL Spell -> expect Dmg stays the same
        public void CalcDamage_AgainstNormalSpell_ReturnsInt()
        {
            //Arrange
            Kraken leviathan = new Kraken();
            NormalSpell enemySpell = new NormalSpell();
            //Act
            int leviathanDamage = leviathan.CalcDamage(enemySpell);
            int expectedDamage = 30;
            //Assert
            Assert.AreEqual(expectedDamage, leviathanDamage);
        }

        [Test] // Enemy NORMAL Spell against Kraken -> expect 0 enemy Dmg
        public void CalcEnemyDamage_AgainstNORMALSpell_ReturnsInt()
        {
            //Arrange
            Kraken leviathan = new Kraken();
            NormalSpell enemySpell = new NormalSpell();
            //Act
            int enemyDamage = enemySpell.CalcDamage(leviathan);
            int expectedDamage = 0;
            //Assert
            Assert.AreEqual(expectedDamage, enemyDamage);
        }

        [Test] // Enemy WATER Spell against Kraken -> expect 0 enemy Dmg
        public void CalcEnemyDamage_AgainstWATERSpell_ReturnsInt()
        {
            //Arrange
            Kraken leviathan = new Kraken();
            WaterSpell enemySpell = new WaterSpell();
            //Act
            int enemyDamage = enemySpell.CalcDamage(leviathan);
            int expectedDamage = 0;
            //Assert
            Assert.AreEqual(expectedDamage, enemyDamage);
        }

        [Test] // Enemy FIRE Spell against Kraken -> expect 0 enemy Dmg
        public void CalcEnemyDamage_AgainstFIRESpell_ReturnsInt()
        {
            //Arrange
            Kraken leviathan = new Kraken();
            FireSpell enemySpell = new FireSpell();
            //Act
            int enemyDamage = enemySpell.CalcDamage(leviathan);
            int expectedDamage = 0;
            //Assert
            Assert.AreEqual(expectedDamage, enemyDamage);
        }
    }
}