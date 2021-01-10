using NUnit.Framework;
using MCTG_Bernacki;
using System.Net.Http.Headers;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    public class WizzardTests
    {
        [Test]
        public void Constructor_Base_ReturnsTrue()
        {
            //Arrange
            Wizzard wiz = new Wizzard();

            // Assert
            Assert.IsTrue(wiz.Name == "Merrlin");
        }

        [Test]
        public void Constructor_WithParameters_ReturnsTrue()
        {
            //Arrange
            Wizzard wiz = new Wizzard("Morgane", 300, element.FIRE, 80);

            //Assert
            Assert.AreEqual(wiz.Name, "Morgane");
        }
        [Test] // Wizzard against MonsterCard -> expect Dmg stays the same
        public void CalcDamage_AgainstMonsterCard_ReturnsInt()
        {
            //Arrange
            Wizzard wiz = new Wizzard();
            Wizzard evilwiz = new Wizzard();
            //Act
            int wizDamage = wiz.CalcDamage(evilwiz);
            //Assert
            Assert.AreEqual(wizDamage, wiz.Damage);
        }

        [Test] // Enemy Orc against Wizzard -> expect 0 enemy Dmg
        public void CalcEnemyDamage_AgainstOrc_ReturnsInt()
        {
            //Arrange
            Wizzard wiz = new Wizzard();
            Orc orkbolg = new Orc();
            //Act
            int enemyDamage = wiz.CalcEnemyDamage(orkbolg);
            int expectedDamage = 0;
            //Assert
            Assert.AreEqual(expectedDamage, enemyDamage);
        }

        [Test] // Wizzard against FIRE Spell -> expect Dmg stay the same
        public void CalcDamage_AgainstFireSpell_ReturnsInt()
        {
            //Arrange
            Wizzard wiz = new Wizzard();
            FireSpell enemySpell = new FireSpell();
            //Act
            int wizDamage = wiz.CalcDamage(enemySpell);
            int expectedDamage = 12;
            //Assert
            Assert.AreEqual(expectedDamage, wizDamage);
        }

        [Test] // Wizzard against WATER Spell -> expect Dmg halved
        public void CalcDamage_AgainstWaterSpell_ReturnsInt()
        {
            //Arrange
            Wizzard wiz = new Wizzard();
            WaterSpell enemySpell = new WaterSpell();
            //Act
            int wizDamage = wiz.CalcDamage(enemySpell);
            int expectedDamage = 6;
            //Assert
            Assert.AreEqual(expectedDamage, wizDamage);
        }

        [Test] // Wizzard against NORMAL Spell -> expect Dmg doubled
        public void CalcDamage_AgainstNormalSpell_ReturnsInt()
        {
            //Arrange
            Wizzard wiz = new Wizzard();
            NormalSpell enemySpell = new NormalSpell();
            //Act
            int wizDamage = wiz.CalcDamage(enemySpell);
            int expectedDamage = 24;
            //Assert
            Assert.AreEqual(expectedDamage, wizDamage);
        }
    }
}