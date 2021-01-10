using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public class ElectroSpell : SpellCard
    {
        public ElectroSpell(string name, int price, element e, int damage) : base(name, price, e, damage)
        {

        }

        public ElectroSpell() : base("Electric Discharge", 60, element.ELECTRO, 30)
        {

        }
    }
}
