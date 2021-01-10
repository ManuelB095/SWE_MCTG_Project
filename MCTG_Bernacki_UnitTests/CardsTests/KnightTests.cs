using NUnit.Framework;
using MCTG_Bernacki;
using System.Net.Http.Headers;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    public class KnightTests
    {
        [Test]
        public void Constructor_Base_ReturnsTrue()
        {
            //Arrange
            Knight paladin = new Knight();

            // Assert
            Assert.IsTrue(paladin.Name == "Lancelot");
        }

        [Test]
        public void Constructor_WithParameters_ReturnsTrue()
        {
            //Arrange
            Knight paladin = new Knight("SirBertram", 80, element.NORMAL, 20);

            //Assert
            Assert.AreEqual(paladin.Name, "SirBertram");
        }
        [Test] // Knight against MonsterCard -> expect Dmg stays the same
        public void CalcDamage_AgainstMonsterCard_ReturnsInt()
        {
            //Arrange
            Knight paladin = new Knight();
            Knight evilPaladin = new Knight();
            //Act
            int paladinDamage = paladin.CalcDamage(evilPaladin);
            //Assert
            Assert.AreEqual(paladinDamage, paladin.Damage);
        }              

        [Test] // Knight against FIRE Spell -> expect Dmg halved
        public void CalcDamage_AgainstFireSpell_ReturnsInt()
        {
            //Arrange
            Knight paladin = new Knight();
            FireSpell enemySpell = new FireSpell();
            //Act
            int paladinDamage = paladin.CalcDamage(enemySpell);
            int expectedDamage = 5;
            //Assert
            Assert.AreEqual(expectedDamage, paladinDamage);
        }

        [Test] // Knight against WATER Spell -> 0 damage
        public void CalcDamage_AgainstWaterSpell_ReturnsInt()
        {
            //Arrange
            Knight paladin = new Knight();
            WaterSpell enemySpell = new WaterSpell();
            //Act
            int paladinDamage = paladin.CalcDamage(enemySpell);
            int expectedDamage = 0;
            //Assert
            Assert.AreEqual(expectedDamage, paladinDamage);
        }

        [Test] // Knight against NORMAL Spell -> expect Dmg stays the same
        public void CalcDamage_AgainstNormalSpell_ReturnsInt()
        {
            //Arrange
            Knight paladin = new Knight();
            NormalSpell enemySpell = new NormalSpell();
            //Act
            int paladinDamage = paladin.CalcDamage(enemySpell);
            int expectedDamage = 10;
            //Assert
            Assert.AreEqual(expectedDamage, paladinDamage);
        }
    }
}