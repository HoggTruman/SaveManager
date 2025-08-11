using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace SaveManager.Services;

public class AppdataService
{
    private const string AppdataDirectoryName = "Save Manager";
    private static readonly string AppdataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppdataDirectoryName);
    private static readonly string AppdataPath = Path.Combine(AppdataFolder, "appdata.xml");

    private XElement _tree = new("Appdata", new XElement("Games"), new XElement("Settings"));

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
