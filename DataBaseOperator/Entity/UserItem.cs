using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseOperator.Entities;

[Table("users")]
public class UserItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("ID")]
    public long ID {get;set;}

    [Column("Name")]
    public string Name {get;set;} = "UNKNOWN";

    [Column("Region")]
    public string Region {get;set;} = "kg";

    [Column("State")]
    public string State {get;set;} = "hub";

    public UserItem() {}

    public UserItem(string name, long id, string region, string state)
    {
        Name = name;
        ID = id;
        Region = region;
        State = state;
    }
}