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
            return enemy.Damage;
        }
    }
}
