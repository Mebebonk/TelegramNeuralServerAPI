using SettingsGenerator;

namespace CustomSettingsGenerator
{
	public class CustomJsonSettingsAttribute : JsonSettingsAttribute
	{
		public CustomJsonSettingsAttribute(string fileName, bool includeProperties = false) : base(fileName, includeProperties) { }
		
		public CustomJsonSettingsAttribute(string fileName, Type attributeLink, bool includeProperties = false) : base(fileName, attributeLink, includeProperties) { }
		
		protected override string RealizeFilePath(string path)
		{
			DirectoryInfo dirNfo = new(@".\settings");
			if (!dirNfo.Exists) { dirNfo.Create(); }

			return $"settings\\{path}.json";
		}
	}
}
