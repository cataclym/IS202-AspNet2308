using System.ComponentModel.DataAnnotations;

public class County 
{
    [Key]
    public int CountyId { get; set; }
    [MaxLength(15)]
    public required string Name { get; set; }
}