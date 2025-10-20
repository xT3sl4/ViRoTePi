using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inpost.classes
{
    public class Client : User
    {
        private string payment_method;
        public Client(int id, string name, string email, Password password, DateTime date, string role, string phoneNumber, string imageUrl, bool sex) : base(id, name, email, password, date, role, phoneNumber, imageUrl, sex)
        {   
        }
        private bool sign_up()
        {
            return true;
        }
    }
}
