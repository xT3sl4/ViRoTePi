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
        public int Id;
        private string Name;
        private string Email;
        private Password Password;
        private DateTime Date;
        private string Role;
        private string PhoneNumber;
        private string ImageUrl;
        private bool Sex;

        public User(int id, string name, string email, Password password, DateTime date, string role, string phoneNumber, string imageUrl, bool sex)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            Date = date;
            Role = role;
            PhoneNumber = phoneNumber;
            ImageUrl = imageUrl;
            Sex = sex;
        }

        private string sign_in()
        {
            return Role;
        }

    }
}
