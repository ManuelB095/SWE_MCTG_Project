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
                return this.CalcDamageVsSpells(enemy);
            }
            return -1; // Throw error instead ?
        }

    }
}
