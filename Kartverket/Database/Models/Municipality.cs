using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Municipality 
{
    [Key]
    public int MunicipalityId { get; set; }
    [MaxLength(15)]
    public string Name { get; set; }
    [ForeignKey("CountyId")]
    public int CountyId { get; set; }
    public County County { get; set; }
}