using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MCTG_Bernacki
{
    [TestFixture]
    class DragonTests
    {
        [Test]
        public void Constructor_Base_ReturnsTrue()
        {
            // Arrange
            Dragon smaug = new Dragon();
            Dragon guams = new Dragon();

            bool sameObject = smaug.Equals(guams);
            Assert.IsTrue(sameObject);          
        }
        
    }
}
