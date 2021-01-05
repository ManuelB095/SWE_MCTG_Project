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
                return this.CalcDamageVsSpells(enemy);
            }
            return -1; // Throw error instead ?
        }       
    }
}
