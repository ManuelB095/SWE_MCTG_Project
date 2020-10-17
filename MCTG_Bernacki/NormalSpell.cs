using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public class NormalSpell : SpellCard
    {
        public NormalSpell(string name, int price, element e, int damage) : base(name, price, e, damage)
        {

        }

        public NormalSpell() : base("Magician`s Fist", 15, element.NORMAL, 40)
        {

        }
    }
}
