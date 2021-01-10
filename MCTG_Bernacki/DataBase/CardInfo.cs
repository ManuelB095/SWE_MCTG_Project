using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public class CardInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public string Element { get; set; }

        public int Damage { get; set; }
        public int Price { get; set; }

        public CardInfo(String name, String type, String element, int damage, int price)
        {
            this.Name = name;
            this.Type = type;
            this.Element = element;
            this.Damage = damage;
            this.Price = price;
        }

    }
}
