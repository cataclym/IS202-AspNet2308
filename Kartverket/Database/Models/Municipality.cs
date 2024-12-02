using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Municipality 
{
    [Key]
    public int MunicipalityId { get; set; }
    [MaxLength(15)]
    public required string Name { get; set; }
    [ForeignKey("CountyId")]
    public int CountyId { get; set; }
    public required County County { get; set; }
}