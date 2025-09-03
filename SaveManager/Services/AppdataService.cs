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
    /// Replaces the games in the appdata file with those provided. 
    /// </summary>
    /// <param name="newGames"></param>
    /// <exception cref="AppdataException"/>
    public void ReplaceGames(IEnumerable<Game> newGames)
    {
        ModifyDocument(doc => ReplaceGamesInDocument(doc, newGames));
    }


    /// <summary>
    /// Retrieves games from the appdata file.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="AppdataException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public IEnumerable<Game> GetGames()
    {
        try
        {
            XElement gamesElement = _document.Root!.Element(GamesName)!;
            return gamesElement.Elements().Select(ConvertFromXElement<GameAppdataDTO>)
                .Select(x => new Game(x.Name, x.SavefileLocation, x.ProfilesDirectory));
        }
        catch (ValidationException)
        {
            throw new AppdataException("Invalid appdata found\n Do not edit appdata.xml manually.");
        }        
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


    internal static void ReplaceGamesInDocument(XDocument document, IEnumerable<Game> newGames)
    {
        XElement newGamesElement = new(GamesName, newGames.Select(x => ConvertToXElement(x.ToGameAppdataDTO())));
        XElement? gamesElement = document.Element(RootName)!.Element(GamesName)!;
        gamesElement.ReplaceWith(newGamesElement);
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
