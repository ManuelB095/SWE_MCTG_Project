using NUnit.Framework;
using MCTG_Bernacki;
using System.Net.Http.Headers;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    public class NormalSpellTests
    {
        [Test]
        public void Constructor_Base_ReturnsTrue()
        {
            //Arrange
            NormalSpell norm = new NormalSpell();

            // Assert
            Assert.IsTrue(norm.Name == "Magician`s Fist");
        }

        [Test]
        public void Constructor_WithParameters_ReturnsTrue()
        {
            //Arrange
            NormalSpell norm = new NormalSpell("Norm", 200, element.FIRE, 60);

            //Assert
            Assert.AreEqual(norm.Name, "Norm");
        }
        
        [Test] // NormalSpell against FIRE Type -> expect Dmg halved
        public void CalcDamage_AgainstFireSpell_ReturnsInt()
        {
            //Arrange
            NormalSpell norm = new NormalSpell();
            FireSpell enemySpell = new FireSpell();
            //Act
            int normDamage = norm.CalcDamage(enemySpell);
            int expectedDamage = 20;
            //Assert
            Assert.AreEqual(expectedDamage, normDamage);
        }

        [Test] // NormalSpell against WATER Spell -> expect Dmg doubled
        public void CalcDamage_AgainstWaterSpell_ReturnsInt()
        {
            //Arrange
            NormalSpell norm = new NormalSpell();
            WaterSpell enemySpell = new WaterSpell();
            //Act
            int normDamage = norm.CalcDamage(enemySpell);
            int expectedDamage = 80;
            //Assert
            Assert.AreEqual(expectedDamage, normDamage);
        }

        [Test] // NormalSpell against NORMAL Spell -> expect Dmg stays the same
        public void CalcDamage_AgainstNormalSpell_ReturnsInt()
        {
            //Arrange
            NormalSpell norm = new NormalSpell();
            NormalSpell enemySpell = new NormalSpell();
            //Act
            int normDamage = norm.CalcDamage(enemySpell);
            int expectedDamage = 40;
            //Assert
            Assert.AreEqual(expectedDamage, normDamage);
        }
    }
}