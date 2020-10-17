using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MCTG_Bernacki
{
    public class Knight : MonsterCard
    {
        public Knight(string name, int price, element e, int damage) : base(name, price, e, damage)
        {

        }
        public Knight() : base("Lancelot", 40, element.NORMAL, 10)
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
                if (enemy.Element == element.WATER) // Knights drown in WATER
                    return 0;
                else if (enemy.Element == element.FIRE)
                    return (int)Math.Floor((double)this.Damage/2);
                else
                    return this.Damage;
            }
            return -1; // Throw error instead ?
        }
    }
}
