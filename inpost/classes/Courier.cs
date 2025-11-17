using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.ObjectModel;

namespace inpost.classes
{
    internal class Courier : User
    {
        public string Region;
        private ObservableCollection<Package> packages;
        public Courier(int id, string name, string email, string password, DateTime date, string role, string phoneNumber, string imageUrl, bool sex, string region) : base(id,name,email,password,date,role,phoneNumber,imageUrl,sex)
        {
            this.Region = region;
        }
    }
}
