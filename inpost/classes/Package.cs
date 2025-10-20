using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inpost.classes
{
    internal class Package
    {
        public string Id;
        private string Size;
        private string ClientId;
        private bool Delivered = false;
        private DateTime ArrivalDate;
        private DateTime PickupDeadline;
    }
}
