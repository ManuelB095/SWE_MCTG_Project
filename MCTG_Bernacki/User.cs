using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MCTG_Bernacki
{
    class User
    {
        public string Username { get; private set; }
        public List<Card> AllCards { get; private set; }

        public User(string username)
        {
            this.setUsername(username);
        }

        public void setUsername(string username)
        {
            if(username.Length <= 40)
            {
                this.Username = username;
            }
            else
            {
                throw new ArgumentException("SetUsername: Uername can only be 40 characters long!");
            }

            if(!Regex.IsMatch(username, "^[a-zA-Z0-9]*$"))
            {
                throw new ArgumentException("SetUsername: Uername can only contain alphanumericals!");
            }
        }

        public void CreateEmptyStack()
        {
            this.AllCards = new List<Card>();
        }

        public void AddCard(Card newCard)
        {
           this.AllCards.Add(newCard);
        }

    }
}
