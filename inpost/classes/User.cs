using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace inpost.classes
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public DateTime Date_Of_Birth { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Phone_Number { get; set; }
        public string ImageUrl { get; set; }
        public bool Sex { get; set; }

        public User() { }

        public User(int id, string name, string email, string password, DateTime date, string role, string phoneNumber, string imageUrl, bool sex)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            Date_Of_Birth = date;
            Role = role;
            Phone_Number = phoneNumber;
            ImageUrl = imageUrl;
            Sex = sex;
        }

        private string sign_in()
        {
            return Role;
        }

    }
}
