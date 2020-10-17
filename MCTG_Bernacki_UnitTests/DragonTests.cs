using NUnit.Framework;
using MCTG_Bernacki;
using System.Net.Http.Headers;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    public class DragonTests
    {
        [Test]
        public void Constructor_Base_ReturnsTrue()
        {
            //Arrange
            Dragon smaug = new Dragon();

            // Assert
            Assert.IsTrue(smaug.Name == "Red Dragon");
        }

        [Test]
        public void Constructor_WithParameters_ReturnsTrue()
        {
            //Arrange
            Dragon smaug = new Dragon("smaug", 200, element.FIRE, 60);

            //Assert
            Assert.AreEqual(smaug.Name, "smaug");
        }
        [Test] // Dragon against MonsterCard -> expect Dmg stays the same
        public void CalcDamage_AgainstMonsterCard_ReturnsInt()
        {
            //Arrange
            Dragon smaug = new Dragon();
            Dragon guams = new Dragon();
            //Act
            int smaugDamage = smaug.CalcDamage(guams);
            //Assert
            Assert.AreEqual(smaugDamage, smaug.Damage);
        }

        [Test] // Dragon against FIRE Spell -> expect Dmg stays the same
        public void CalcDamage_AgainstFireSpell_ReturnsInt()
        {
            //Arrange
            Dragon smaug = new Dragon();
            FireSpell enemySpell = new FireSpell();
            //Act
            int smaugDamage = smaug.CalcDamage(enemySpell);
            int expectedDamage = 15;
            //Assert
            Assert.AreEqual(expectedDamage, smaugDamage);
        }

        [Test] // Dragon against WATER Spell -> expect Dmg halved
        public void CalcDamage_AgainstWaterSpell_ReturnsInt()
        {
            //Arrange
            Dragon smaug = new Dragon();
            WaterSpell enemySpell = new WaterSpell();
            //Act
            int smaugDamage = smaug.CalcDamage(enemySpell);
            int expectedDamage = 7;
            //Assert
            Assert.AreEqual(expectedDamage, smaugDamage);
        }

        [Test] // Dragon against NORMAL Spell -> expect Dmg doubled
        public void CalcDamage_AgainstNormalSpell_ReturnsInt()
        {
            //Arrange
            Dragon smaug = new Dragon();
            NormalSpell enemySpell = new NormalSpell();
            //Act
            int smaugDamage = smaug.CalcDamage(enemySpell);
            int expectedDamage = 30;
            //Assert
            Assert.AreEqual(expectedDamage, smaugDamage);
        }
    }
}