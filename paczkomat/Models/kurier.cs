using System;
using System.Collections.Generic;

namespace paczkomat.Models;

public partial class kurier
{
    public int kurier_id { get; set; }

    public int? user_id { get; set; }

    public string? state { get; set; }

    public virtual user? user { get; set; }
}
