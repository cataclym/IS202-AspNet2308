using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Models;

[Table("Messages")]
public class MessagesModel
{
    public int MessageId { get; set; }
    public string Message { get; set; }
}