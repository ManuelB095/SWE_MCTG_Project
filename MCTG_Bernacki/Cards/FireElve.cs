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
                return this.CalcDamageVsSpells(enemy);
            }
            return -1; // Throw error instead ?
        }        
    }
}
