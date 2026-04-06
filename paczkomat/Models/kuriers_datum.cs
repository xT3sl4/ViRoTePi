using System;
using System.Collections.Generic;

namespace paczkomat.Models;

public partial class kuriers_datum
{
    public int? kurier_id { get; set; }

    public int? pack_id { get; set; }

    public virtual kurier? kurier { get; set; }

    public virtual pack? pack { get; set; }
}
