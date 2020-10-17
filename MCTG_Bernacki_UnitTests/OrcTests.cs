using NUnit.Framework;
using MCTG_Bernacki;
using System.Net.Http.Headers;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    public class OrcTests
    {
        [Test]
        public void Constructor_Base_ReturnsTrue()
        {
            //Arrange
            Orc justOrc = new Orc();

            // Assert
            Assert.IsTrue(justOrc.Name == "Orc-Warrior");
        }

        [Test]
        public void Constructor_WithParameters_ReturnsTrue()
        {
            //Arrange
            Orc justOrc = new Orc("WhoThis?", 30, element.NORMAL, 17);

            //Assert
            Assert.AreEqual(justOrc.Name, "WhoThis?");
        }
        [Test] // Orc against MonsterCard -> expect Dmg stays the same
        public void CalcDamage_AgainstMonsterCard_ReturnsInt()
        {
            //Arrange
            Orc justOrc = new Orc();
            Orc eviljustOrc = new Orc();
            //Act
            int justOrcDamage = justOrc.CalcDamage(eviljustOrc);
            //Assert
            Assert.AreEqual(justOrcDamage, justOrc.Damage);
        }

        [Test] // Enemy Wizzard against Orc -> expect 0 Dmg
        public void CalcEnemyDamage_AgainstOrc_ReturnsInt()
        {
            //Arrange
            Orc justOrc = new Orc();
            Wizzard wiz = new Wizzard();
            //Act
            int justOrcDamage = justOrc.CalcDamage(wiz);
            int expectedDamage = 0;
            //Assert
            Assert.AreEqual(expectedDamage, justOrcDamage);
        }

        [Test] // Orc against FIRE Spell -> expect Dmg halved
        public void CalcDamage_AgainstFireSpell_ReturnsInt()
        {
            //Arrange
            Orc justOrc = new Orc();
            FireSpell enemySpell = new FireSpell();
            //Act
            int justOrcDamage = justOrc.CalcDamage(enemySpell);
            int expectedDamage = 4;
            //Assert
            Assert.AreEqual(expectedDamage, justOrcDamage);
        }

        [Test] // Orc against WATER Spell -> expect Dmg doubled
        public void CalcDamage_AgainstWaterSpell_ReturnsInt()
        {
            //Arrange
            Orc justOrc = new Orc();
            WaterSpell enemySpell = new WaterSpell();
            //Act
            int justOrcDamage = justOrc.CalcDamage(enemySpell);
            int expectedDamage = 16;
            //Assert
            Assert.AreEqual(expectedDamage, justOrcDamage);
        }

        [Test] // Orc against NORMAL Spell -> expect Dmg stays the same
        public void CalcDamage_AgainstNormalSpell_ReturnsInt()
        {
            //Arrange
            Orc justOrc = new Orc();
            NormalSpell enemySpell = new NormalSpell();
            //Act
            int justOrcDamage = justOrc.CalcDamage(enemySpell);
            int expectedDamage = 8;
            //Assert
            Assert.AreEqual(expectedDamage, justOrcDamage);
        }
    }
}