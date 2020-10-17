using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MCTG_Bernacki
{
    public class Wizzard : MonsterCard
    {
        public Wizzard(string name, int price, element e, int damage) : base(name, price, e, damage)
        {

        }
        public Wizzard() : base("Merrlin", 50, element.FIRE, 12)
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
                    return (int)Math.Floor((double)this.Damage / 2); // double bc otherwise ambiguous
                if (enemy.Element == element.NORMAL)
                    return this.Damage * 2;
                else
                    return this.Damage;
            }
            return -1; // Throw error instead ?
        }

        public override int CalcEnemyDamage(Card enemy)
        {
            if (enemy is Orc)
                return 0;
            else
                return enemy.Damage;
        }

    }
}
