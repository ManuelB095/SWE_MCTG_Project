using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public class ElectroCloud : MonsterCard
    {
        public ElectroCloud(string name, int price, element e, int damage) : base(name, price, e, damage)
        {

        }
        public ElectroCloud() : base("CloudGhast", 60, element.ELECTRO, 27)
        {

        }

        public override int CalcDamage(Card enemy)
        {
            if (enemy is MonsterCard)
            {
                if (enemy is Wizzard) // Wizzards can shield themselves with magic
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
