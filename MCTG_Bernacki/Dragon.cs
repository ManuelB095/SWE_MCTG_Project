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
            if(enemy is FireElve)
            {
                return 0;
            }
            else if (enemy is MonsterCard)
            {
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
