using System.Xml.Serialization;

namespace SaveManager.DTOs;

[XmlType(TypeName = "Game")]
public class GameDTO
{
    public required string Name { get; set; }
    public string? SavefileLocation { get; set; }
    public string? ProfilesDirectory { get; set; }    
}
