using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class Sale
{
    [Key]
    public long Id { get; set; }

    [Column(TypeName = "varchar(250)")]
    public string? ItemName { get; set; } = string.Empty;

    public int? ItemAmount { get; set; }

    [Column(TypeName = "varchar(250)")]
    public string? Location { get; set; } = string.Empty;

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public bool? IsActive { get; set; } = true;

    public DateTime? LastModifiedOn { get; set; }

    public long? LastModifiedBy { get; set; }
}
