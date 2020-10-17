using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MCTG_Bernacki
{
    public class Dragon : MonsterCard
    {
        public Dragon(string name, int price, element e, int damage) : base(name, price, e, damage)
        {

        }

        public Dragon() : base("Red Dragon", 100, element.FIRE, 15)
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
                if (enemy.Element == element.WATER) // Dragons are weak agains WATER
                    return (int) Math.Floor((double)this.Damage/2); // double bc otherwise ambiguous
                if (enemy.Element == element.NORMAL) // Dragons are strong agains NORMAL
                    return this.Damage * 2;
                else
                    return this.Damage;
            }
            return -1; // Throw error instead ?
        }

    }
}
