using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MCTG_Bernacki
{
    public class Orc : MonsterCard
    {
        public Orc(string name, int price, element e, int damage) : base(name, price, e, damage)
        {

        }
        public Orc() : base("Orc-Warrior", 20, element.NORMAL, 8)
        {

        }

        public override int CalcDamage(Card enemy)
        {
            if (enemy is MonsterCard)
            {
                if (enemy is Wizzard) // Wizzards can control Orcs !
                    return 0;
                else
                    return this.Damage;
            }
            else if (enemy is SpellCard)
            {
                if (enemy.Element == element.FIRE)
                    return (int)Math.Floor((double)this.Damage / 2);
                if (enemy.Element == element.WATER)
                    return this.Damage * 2;
                else
                    return this.Damage;
            }
            return -1; // Throw error instead ?
        }

    }
}
