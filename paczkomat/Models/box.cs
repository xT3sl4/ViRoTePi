using System;
using System.Collections.Generic;

namespace paczkomat.Models;

public partial class box
{
    public int box_id { get; set; }

    public int? pack_id { get; set; }

    public string? size { get; set; }

    public virtual pack? pack { get; set; }
}
