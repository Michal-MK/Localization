using System.Collections.Generic;

namespace Igor.Localization {
	internal class LocaleData {
		public string LanguageCode { get; set; }

		public string FullLangName { get; set; }

		public Dictionary<int, string> Language { get; set; }
	}
}
