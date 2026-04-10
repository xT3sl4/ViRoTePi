using System;
using System.Collections.Generic;

namespace paczkomat.Models;

public partial class user
{
    public int id { get; set; }

    public string? name { get; set; }

    public string? surname { get; set; }

    public string? email { get; set; }

    public DateOnly? date_of_birth { get; set; }

    public string? password { get; set; }

    public string? role { get; set; }

    public int? phone_number { get; set; }

    public string? selfie { get; set; }

    public bool? sex { get; set; }

    public virtual ICollection<admin> admins { get; set; } = new List<admin>();

    public virtual ICollection<klient> klients { get; set; } = new List<klient>();

    public virtual ICollection<kurier> kuriers { get; set; } = new List<kurier>();
}
