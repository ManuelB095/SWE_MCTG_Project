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
            if(enemy is Kraken)
            {
                return 0;
            }

            if (this.Element == element.WATER)
            {
                if (enemy.Element == element.FIRE)
                    return this.Damage * 2;
                else if (enemy.Element == element.NORMAL)
                    return (int)Math.Floor((double)this.Damage / 2);
                else if (enemy.Element == element.ELECTRO)
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
                else if (enemy.Element == element.ELECTRO)
                    return this.Damage;
                else
                    return this.Damage;
            }
            else if (this.Element == element.NORMAL)
            {
                if (enemy.Element == element.WATER)
                    return this.Damage * 2;
                else if (enemy.Element == element.FIRE)
                    return (int)Math.Floor((double)this.Damage / 2);
                else if (enemy.Element == element.ELECTRO)
                    return this.Damage * 2;
                else
                    return this.Damage;
            }
            else if (this.Element == element.ELECTRO)
            {
                Random rnd = new Random();
                double factor = rnd.Next(5, 25); // Random int between 5 and 25
                if (enemy.Element == element.WATER)
                    return (int)Math.Floor((double)this.Damage * (factor / 5.0));
                else if (enemy.Element == element.FIRE)
                    return (int)Math.Floor((double)this.Damage * (factor / 10.0));
                else if (enemy.Element == element.NORMAL)
                    return (int)Math.Floor((double)this.Damage * (factor / 20.0));
                else
                    return this.Damage;
            }
            return -1; // Some Error Occurred.
        }               
    }
}
