using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MCTG_Bernacki
{
    public class Kraken : MonsterCard
    {
        public Kraken(string name, int price, element e, int damage) : base(name, price, e, damage)
        {

        }
        public Kraken() : base("Leviathan", 200, element.WATER, 30)
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
                return this.Damage;
            }
            return -1; // Throw error instead ?
        }

        public override int CalcEnemyDamage(Card enemy)
        {
            if (enemy is SpellCard) // Kraken is immune to Spell Cards.
            {
                return 0;
            }
            else
                return enemy.Damage;
        }

    }
}
