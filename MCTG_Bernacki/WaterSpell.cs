using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public class WaterSpell : SpellCard
    {
        public WaterSpell(string name, int price, element e, int damage) : base(name,price,e,damage)
        {

        }

        public WaterSpell() : base("Splash", 15, element.WATER, 40)
        {

        }
    }
}
