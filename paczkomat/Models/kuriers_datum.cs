using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace paczkomat.Models;

public partial class kuriers_datum
{
    [Key, Column(Order = 0)]
    public int kurier_id { get; set; }

    [Key, Column(Order = 1)]
    public int pack_id { get; set; }   

    public virtual kurier? kurier { get; set; }
    public virtual pack? pack { get; set; }
}