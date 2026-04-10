using System;
namespace paczkomat.Models;

public partial class pending_pack
{
    public int pending_id { get; set; }
    public int sender_klient_id { get; set; }
    public int receiver_klient_id { get; set; }
    public string size { get; set; }
    public int paczkomat_id { get; set; }
    public DateTime? created_at { get; set; }
    public string status { get; set; } = "waiting";

    public virtual klient sender_klient { get; set; }
    public virtual klient receiver_klient { get; set; }
    public virtual paczkomat paczkomat { get; set; }
}