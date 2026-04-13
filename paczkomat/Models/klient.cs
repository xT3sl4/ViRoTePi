using System;
using System.Collections.Generic;

namespace paczkomat.Models;

public partial class klient
{
    public int klient_id { get; set; }

    public int? user_id { get; set; }

    public int? pack_id { get; set; }

    public virtual ICollection<pack> packs { get; set; } = new List<pack>();

    public virtual user? user { get; set; }
}
