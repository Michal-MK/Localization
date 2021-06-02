using System.Collections.Generic;

namespace Igor.Localization {
	internal class LocaleData {
		internal string LanguageCode { get; set; }

		internal string FullLangName { get; set; }

		internal Dictionary<int, string> Language { get; set; }
	}
}
