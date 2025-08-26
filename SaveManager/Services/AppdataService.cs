using SaveManager.DTOs;
using SaveManager.Exceptions;
using SaveManager.Models;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace SaveManager.Services;

public class AppdataService
{
    internal const string RootName = "Appdata";
    internal const string GamesName = "Games";
    internal const string SettingsName = "Settings";
    private const string AppdataDirectoryName = "Save Manager";

    private static readonly string AppdataFolder;
    private static readonly string AppdataPath;

    internal static XDocument DefaultDocument => new(new XElement(RootName, new XElement(GamesName), new XElement(SettingsName)));

    private static XDocument _document = DefaultDocument;

    static AppdataService()
    {
        string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppdataDirectoryName);
        #if DEBUG
            AppdataFolder = Path.Combine(basePath, "Debug");
        #else
            AppdataFolder = Path.Combine(basePath, "Release");
        #endif

        AppdataPath = Path.Combine(AppdataFolder, "appdata.xml");
        Initialize();
    }    
      


    /// <summary>
    /// Loads / creates the appdata file.
    /// </summary>
    /// <exception cref="AppdataException"/>
    internal static void Initialize()
    {
        try
        {
            if (!Directory.Exists(AppdataFolder))
            {
                Directory.CreateDirectory(AppdataFolder);
            }

            if (Path.Exists(AppdataPath))
            {
                _document = XDocument.Load(AppdataPath);         
            }
            else
            {
                _document.Save(AppdataPath);
            }
        }
        catch (Exception e)
        {
            throw new AppdataException(e.Message, e.InnerException);
        }        
    }



    /// <summary>
    /// Adds a new game to the appdata file.
    /// </summary>
    /// <param name="game"></param> 
    /// <exception cref="AppdataException"/>
    public void AddGame(Game game)
    {
        ModifyDocument(doc => AddGameToDocument(doc, game));
    }


    /// <summary>
    /// Renames a game in the appdata file.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="newName"></param>
    /// <exception cref="AppdataException"/>
    public void RenameGame(Game game, string newName)
    {
        ModifyDocument(doc => RenameGameInDocument(doc, game, newName));
    }


    /// <summary>
    /// Deletes a game in the appdata file.
    /// </summary>
    /// <param name="game"></param>
    /// <exception cref="AppdataException"/>
    public void DeleteGame(Game game)
    {
        ModifyDocument(doc => DeleteGameInDocument(doc, game));
    }


    /// <summary>
    /// Retrieves games from the appdata file.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Game> GetGames()
    {
        XElement gamesElement = _document.Root!.Element(GamesName)!;
        return gamesElement.Elements().Select(ConvertFromXElement<GameAppdataDTO>)
            .Select(x => new Game() { Name = x.Name, ProfilesDirectory = x.ProfilesDirectory, SavefileLocation = x.SavefileLocation });
    }


    /// <summary>
    /// Applies the action to a temporary document and attempts to overwrite appdata.xml.
    /// If successful in saving, then the internal XDocument is overwritten too.
    /// </summary>
    /// <param name="modify"></param>
    /// <exception cref="AppdataException"></exception>
    internal void ModifyDocument(Action<XDocument> modify)
    {
        XDocument newDocument = new(_document);
        modify(newDocument);
        try
        {
            newDocument.Save(AppdataPath);
        }
        catch (Exception e)
        {
            throw new AppdataException(e.Message, e.InnerException);
        }
        
        _document = newDocument;
    }


    internal static void AddGameToDocument(XDocument document, Game game)
    {
        XElement? gamesElement = document.Element(RootName)!.Element(GamesName)!;
        gamesElement.Add(ConvertToXElement(game.ToGameAppdataDTO()));
    }


    internal static void RenameGameInDocument(XDocument document, Game game, string newName)
    {
        XElement nameElement = document.Root!.Element(GamesName)!
            .Descendants()
            .First(x => x.Name.LocalName == nameof(Game.Name) && x.Value == game.Name);

        nameElement.Value = newName;
    }


    internal static void DeleteGameInDocument(XDocument document, Game game)
    {
        XElement gameElement = document.Root!.Element(GamesName)!
            .Elements()
            .First(x => x.Element(nameof(Game.Name))!.Value == game.Name);

        gameElement.Remove();
    }


    internal static XElement ConvertToXElement<T>(T item) where T : notnull
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "Can not convert null to an XElement");
        }

        XDocument document = new();
        XmlSerializer serializer = new(typeof(T));
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("", "");

        using (XmlWriter xmlWriter = document.CreateWriter())
        {
            serializer.Serialize(xmlWriter, item, namespaces);
        }

        return document.Root!;
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
