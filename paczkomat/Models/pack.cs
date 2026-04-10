using System;
using System.Collections.Generic;

namespace paczkomat.Models;

public partial class pack
{
    public int pack_id { get; set; }

    public string? size { get; set; }

    public int? klient_id { get; set; }

    public bool? delivered { get; set; }

    public DateOnly? date { get; set; }

    public DateOnly? to_when { get; set; }

    public virtual ICollection<box> boxes { get; set; } = new List<box>();

    public virtual klient? klient { get; set; }
}
