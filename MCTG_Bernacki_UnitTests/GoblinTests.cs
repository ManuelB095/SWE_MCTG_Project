using NUnit.Framework;
using MCTG_Bernacki;
using System.Net.Http.Headers;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    public class GoblinTests
    {
        [Test]
        public void Constructor_Base_ReturnsTrue()
        {
            //Arrange
            Goblin gobo = new Goblin();

            // Assert
            Assert.IsTrue(gobo.Name == "Goblin");
        }

        [Test]
        public void Constructor_WithParameters_ReturnsTrue()
        {
            //Arrange
            Goblin gobo = new Goblin("Gobo", 20, element.NORMAL, 6);

            //Assert
            Assert.AreEqual(gobo.Name, "Gobo");
        }
        [Test] // Goblin against MonsterCard -> expect Dmg stays the same
        public void CalcDamage_AgainstMonsterCard_ReturnsInt()
        {
            //Arrange
            Goblin gobo = new Goblin();
            Goblin obog = new Goblin();
            //Act
            int goboDamage = gobo.CalcDamage(obog);
            //Assert
            Assert.AreEqual(goboDamage, gobo.Damage);
        }

        [Test] // Goblin against Dragon -> 0 Damage
        public void CalcDamage_AgainstDragon_ReturnsInt()
        {
            //Arrange
            Goblin gobo = new Goblin();
            Dragon smaug = new Dragon();
            //Act
            int goboDamage = gobo.CalcDamage(smaug);
            //Assert
            Assert.AreEqual(0, goboDamage);
        }

        [Test] // Goblin against FIRE Spell -> expect Dmg halved
        public void CalcDamage_AgainstFireSpell_ReturnsInt()
        {
            //Arrange
            Goblin gobo = new Goblin();
            FireSpell enemySpell = new FireSpell();
            //Act
            int goboDamage = gobo.CalcDamage(enemySpell);
            int expectedDamage = 2;
            //Assert
            Assert.AreEqual(expectedDamage, goboDamage);
        }

        [Test] // Goblin against WATER Spell -> expect Dmg doubled
        public void CalcDamage_AgainstWaterSpell_ReturnsInt()
        {
            //Arrange
            Goblin gobo = new Goblin();
            WaterSpell enemySpell = new WaterSpell();
            //Act
            int goboDamage = gobo.CalcDamage(enemySpell);
            int expectedDamage = 10;
            //Assert
            Assert.AreEqual(expectedDamage, goboDamage);
        }

        [Test] // Goblin against NORMAL Spell -> expect Dmg stays the same
        public void CalcDamage_AgainstNormalSpell_ReturnsInt()
        {
            //Arrange
            Goblin gobo = new Goblin();
            NormalSpell enemySpell = new NormalSpell();
            //Act
            int goboDamage = gobo.CalcDamage(enemySpell);
            int expectedDamage = 5;
            //Assert
            Assert.AreEqual(expectedDamage, goboDamage);
        }
    }
}