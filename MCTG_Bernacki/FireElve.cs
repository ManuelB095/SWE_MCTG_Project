using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MCTG_Bernacki
{
    public class FireElve : MonsterCard
    {
        public FireElve(string name, int price, element e, int damage) : base(name, price, e, damage)
        {

        }
        public FireElve() : base("Legolad", 60, element.FIRE, 14)
        {

        }

        public override int CalcDamage(Card enemy)
        {
            if (enemy is MonsterCard)
            {                
                return this.Damage;
            }
            else if (enemy is SpellCard)
            {
                if (enemy.Element == element.WATER)
                    return (int)Math.Floor((double)this.Damage / 2.0);
                else if (enemy.Element == element.NORMAL)
                    return this.Damage * 2;
                else
                    return this.Damage;
            }
            return -1; // Throw error instead ?
        }

        public override int CalcEnemyDamage(Card enemy)
        {
            if (enemy is Dragon) // FireElves evade dragon attacks
                return 0;
            else
                return enemy.Damage;
        }
    }
}
