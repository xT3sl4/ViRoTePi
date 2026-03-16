using System;
using System.Collections.Generic;

namespace paczkomat.Models;

public partial class admin
{
    public int admin_id { get; set; }

    public int? user_id { get; set; }

    public virtual user? user { get; set; }
}
