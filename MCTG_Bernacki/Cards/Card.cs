using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public abstract class Card
    {
        public string Name { get; private set; }
        public int Price { get; private set; }

        public element Element { get; private set; }

        public int Damage { get; private set; }

        public Card(string name, int price, element elemnt, int damage)
        {
            this.SetName(name);
            this.SetPrice(price);
            this.SetElement(elemnt);
            this.SetDamage(damage);
        }

        public void SetName(string name)
        {
            if(name.Length <= 40)
            {
                this.Name = name;
            }
            else
            {
                throw new ArgumentException("SetName: Input must not exceed 40 characters!");
            }
        }

        public void SetPrice(int price)
        {
            if(price >= 0 && price <= 9999)
            {
                this.Price = price;
            }
            else
            {
                throw new ArgumentException("SetPrice: Not a valid input! Card prices can be from 0 to 9999!");
            }
        }

        public void SetElement(element e)
        {
            this.Element = e;
        }

        public void SetDamage(int damage)
        {
            if (damage >= 0 && damage <= 9999)
            {
                this.Damage = damage;
            }
            else
            {
                throw new ArgumentException("SetDamage: Not a valid input! Damage can not be less than 0 or more than 9999!");
            }
        }

        // If enemy is of type Monster, calculate Damage against it
        public virtual int CalcDamage(Card enemy)
        {
            return this.Damage;
        }
        public virtual int CalcEnemyDamage(Card enemy)
        {
            if (enemy is MonsterCard)
                return enemy.Damage;
            else if (enemy is SpellCard)
                return enemy.CalcDamageVsSpells(this);
            else
                return enemy.Damage;
        }

        public virtual int CalcDamageVsSpells(Card enemy)
        {
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
            else if(this.Element == element.ELECTRO)
            {
                Random rnd = new Random();
                double factor = rnd.Next(5, 25); // Random int between 5 and 25
                if (enemy.Element == element.WATER)
                    return (int)Math.Floor((double)this.Damage * (factor/5.0));
                else if (enemy.Element == element.FIRE)
                    return (int)Math.Floor((double)this.Damage * (factor/10.0));
                else if (enemy.Element == element.NORMAL)
                    return (int)Math.Floor((double)this.Damage * (factor/20.0));
                else
                    return this.Damage;
            }
            return -1; // Some Error Occurred.
        }
    }
}
