using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public class FireSpell : SpellCard
    {
        public FireSpell(string name, int price, element e, int damage) : base(name, price, e, damage)
        {

        }

        public FireSpell() : base("Burning Hands", 15, element.FIRE, 40)
        {

        }
    }
}
