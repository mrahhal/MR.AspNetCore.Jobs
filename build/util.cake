public class Util
{
	public Util(ICakeContext context, BuildParameters build)
	{
		Context = context;
		Build = build;
	}

	public ICakeContext Context { get; set; }
	public BuildParameters Build { get; set; }

	public void PrintInfo()
	{
		Context.Information($@"
Version:       {Build.FullVersion()}
Configuration: {Build.Configuration}
");
	}

	public static string CreateStamp()
	{
		var seconds = (long)(DateTime.UtcNow - new DateTime(2017, 1, 1)).TotalSeconds;
		return seconds.ToString().PadLeft(11, (char)'0');
	}

	public string GetProjectSdk(string project)
	{
		var file = Context.File(project);
		var content = System.IO.File.ReadAllText(file.Path.FullPath);

		XmlDocument doc = new XmlDocument();
		doc.LoadXml(content);

		var projectNode = doc.DocumentElement.SelectSingleNode("/Project");
		XmlAttribute sdkAttribute = (XmlAttribute)projectNode.Attributes.GetNamedItem("Sdk");

		return sdkAttribute?.Value;
	}
}
