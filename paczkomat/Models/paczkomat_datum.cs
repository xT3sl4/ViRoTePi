using System;
using System.Collections.Generic;

namespace paczkomat.Models;

public partial class paczkomat_datum
{
    public int? paczkomat_id { get; set; }

    public int? box_id { get; set; }

    public virtual box? box { get; set; }

    public virtual paczkomat? paczkomat { get; set; }
}
