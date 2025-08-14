using SaveManager.Models;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;


namespace SaveManager.Services;

public class AppdataService
{
    internal const string GamesName = "Games";
    internal const string SettingsName = "Settings";
    private const string AppdataDirectoryName = "Save Manager";
    private static readonly string AppdataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppdataDirectoryName);
    private static readonly string AppdataPath = Path.Combine(AppdataFolder, "appdata.xml");

    private XElement _tree = EmptyTree;

    public static XElement EmptyTree => new("Appdata", new XElement(GamesName), new XElement(SettingsName));


    /// <summary>
    /// Ensures the appdata file exists and loads it as an XElement.
    /// </summary>
    /// <exception cref="IOException"/>
    public void Initialize()
    {
        if (!Directory.Exists(AppdataFolder))
        {
            Directory.CreateDirectory(AppdataFolder);
        }

        if (!Path.Exists(AppdataPath))
        {
            _tree.Save(AppdataPath);
        }
        else
        {
            _tree = XElement.Load(AppdataPath);
        }
    }



    /// <summary>
    /// Adds a new game to the appdata file.
    /// </summary>
    /// <param name="game"></param> 
    /// <exception cref="IOException"/>
    public void AddGame(Game game)
    {
        XElement newTree = new(_tree);
        AddGameToTree(newTree, game);
        _tree.Save(AppdataPath);
        _tree = newTree;
    }


    /// <summary>
    /// Renames a game in the appdata file.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="newName"></param>
    /// <exception cref="IOException"/>
    public void RenameGame(Game game, string newName)
    {
        XElement newTree = new(_tree);
        RenameGameInTree(newTree, game, newName);
        _tree.Save(AppdataPath);
        _tree = newTree;
    }


    /// <summary>
    /// Deletes a game in the appdata file.
    /// </summary>
    /// <param name="game"></param>
    /// <exception cref="IOException"/>
    public void DeleteGame(Game game)
    {
        XElement newTree = new(_tree);
        DeleteGameInTree(newTree, game);
        _tree.Save(AppdataPath);
        _tree = newTree;
    }




    internal static void AddGameToTree(XElement tree, Game game)
    {
        XElement gamesElement = tree.Elements().First(x => x.Name.LocalName == GamesName);
        gamesElement.Add(ConvertToXElement(game));
    }


    internal static void RenameGameInTree(XElement tree, Game game, string newName)
    {
        XElement nameElement = tree.Elements()
            .First(x => x.Name.LocalName == GamesName)
            .Descendants()
            .First(x => x.Name.LocalName == nameof(Game.Name) && x.Value == game.Name);

        nameElement.Value = newName;
    }


    internal static void DeleteGameInTree(XElement tree, Game game)
    {
        XElement gameElement = tree.Elements()
            .First(x => x.Name.LocalName == GamesName)
            .Elements()
            .First(x => x.Elements().First(y => y.Name.LocalName == nameof(Game.Name)).Value == game.Name);

        gameElement.Remove();
    }


    internal static XElement ConvertToXElement<T>(T item) where T : notnull
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "Can not convert null to an XElement");
        }

        XDocument doc = new();
        XmlSerializer serializer = new(typeof(T));
        using (XmlWriter xmlWriter = doc.CreateWriter())
        {
            serializer.Serialize(xmlWriter, item);
        }

        return doc.Root!;
    }


    internal static T ConvertFromXElement<T>(XElement element) where T : notnull
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element), "Can not convert null to an object");
        }

        XmlSerializer serializer = new(typeof(T));
        return (T)serializer.Deserialize(element.CreateReader())!;
    }
}
