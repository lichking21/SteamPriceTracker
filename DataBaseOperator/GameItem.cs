using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseOperator.Entities;

[Table("games")]
public class GameItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int Id {get;set;}

    [Column("title")]
    public string Title {get;set;} = "UNKNOWN";
}