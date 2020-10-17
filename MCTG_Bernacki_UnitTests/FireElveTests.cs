using NUnit.Framework;
using MCTG_Bernacki;
using System.Net.Http.Headers;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    public class FireElveTests
    {
        [Test]
        public void Constructor_Base_ReturnsTrue()
        {
            //Arrange
            FireElve elfi = new FireElve();

            // Assert
            Assert.IsTrue(elfi.Name == "Legolad");
        }

        [Test]
        public void Constructor_WithParameters_ReturnsTrue()
        {
            //Arrange
            FireElve elfi = new FireElve("ElrundElf", 600, element.FIRE, 120);

            //Assert
            Assert.AreEqual(elfi.Name, "ElrundElf");
        }
        [Test] // FireElve against MonsterCard -> expect Dmg stays the same
        public void CalcDamage_AgainstMonsterCard_ReturnsInt()
        {
            //Arrange
            FireElve elfi = new FireElve();
            FireElve evilelfi = new FireElve();
            //Act
            int elfiDamage = elfi.CalcDamage(evilelfi);
            //Assert
            Assert.AreEqual(elfiDamage, elfi.Damage);
        }

        [Test] // Enemy Dragon against FireElve -> expect 0 enemy Dmg
        public void CalcEnemyDamage_AgainstOrc_ReturnsInt()
        {
            //Arrange
            FireElve elfi = new FireElve();
            Dragon smaug = new Dragon();
            //Act
            int enemyDamage = elfi.CalcEnemyDamage(smaug);
            int expectedDamage = 0;
            //Assert
            Assert.AreEqual(expectedDamage, enemyDamage);
        }

        [Test] // FireElve against FIRE Spell -> expect Dmg stay the same
        public void CalcDamage_AgainstFireSpell_ReturnsInt()
        {
            //Arrange
            FireElve elfi = new FireElve();
            FireSpell enemySpell = new FireSpell();
            //Act
            int elfiDamage = elfi.CalcDamage(enemySpell);
            int expectedDamage = 14;
            //Assert
            Assert.AreEqual(expectedDamage, elfiDamage);
        }

        [Test] // FireElve against WATER Spell -> expect Dmg halved
        public void CalcDamage_AgainstWaterSpell_ReturnsInt()
        {
            //Arrange
            FireElve elfi = new FireElve();
            WaterSpell enemySpell = new WaterSpell();
            //Act
            int elfiDamage = elfi.CalcDamage(enemySpell);
            int expectedDamage = 7;
            //Assert
            Assert.AreEqual(expectedDamage, elfiDamage);
        }

        [Test] // FireElve against NORMAL Spell -> expect Dmg doubled
        public void CalcDamage_AgainstNormalSpell_ReturnsInt()
        {
            //Arrange
            FireElve elfi = new FireElve();
            NormalSpell enemySpell = new NormalSpell();
            //Act
            int elfiDamage = elfi.CalcDamage(enemySpell);
            int expectedDamage = 28;
            //Assert
            Assert.AreEqual(expectedDamage, elfiDamage);
        }
    }
}