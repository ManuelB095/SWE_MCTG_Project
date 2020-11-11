using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public abstract class SpellCard : Card
    {
        public SpellCard(string name, int price, element e, int damage) : base(name,price,e,damage)
        {

        }

        public override int CalcDamage(Card enemy)
        {
            if (this.Element == element.WATER)
            {
                if (enemy.Element == element.FIRE)
                    return this.Damage * 2;
                else if (enemy.Element == element.NORMAL)
                    return (int)Math.Floor((double)this.Damage / 2);
                else
                    return this.Damage;

            }            
            else if (this.Element == element.FIRE)
            {
                if (enemy.Element == element.NORMAL)
                    return this.Damage * 2;
                else if (enemy.Element == element.WATER)
                    return (int)Math.Floor((double)this.Damage / 2);
                else
                    return this.Damage;
            }
            else if (this.Element == element.NORMAL)
            {
                if (enemy.Element == element.WATER)
                    return this.Damage * 2;
                else if (enemy.Element == element.FIRE)
                    return (int)Math.Floor((double)this.Damage / 2);
                else
                    return this.Damage;
            }            
            return -1; // Some Error Occurred.
        }
    }
}
