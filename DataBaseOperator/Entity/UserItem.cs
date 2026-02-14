using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseOperator.Entities;

[Table("users")]
public class UserItem
{
    [Key]
    [Column("ID")]
    public long ID {get;set;}

    [Column("Name")]
    public string Name {get;set;} = "UNKNOWN";

    public UserItem() {}

    public UserItem(string name)
    {
        Name = name;
    }
}