using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MCTG_Bernacki
{
    public class Goblin : MonsterCard
    {
        public Goblin(string name, int price, element e, int damage) : base(name,price,e,damage)
        {

        }
        public Goblin() : base("Goblin", 20, element.NORMAL, 5)
        {

        }

        public override int CalcDamage(Card enemy)
        {
            if (enemy is MonsterCard)
            {
                if (enemy is Dragon) // Goblin`s are afraid of Dragons !
                    return 0;
                else
                    return this.Damage;
            }
            else if (enemy is SpellCard)
            {
                return this.CalcDamageVsSpells(enemy);
            }
            return -1; // Throw error instead ?
        }

    }
}
