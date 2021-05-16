using System;
using System.Collections.Generic;

namespace Igor.Localization {
	internal class Language {

		public Language(LocaleData data) {
			langCode = data.LanguageCode;

			Fullname = data.FullLangName;
			Mapping = data.Language;
			ReverseMapping = new Dictionary<string, int>();
			foreach (var item in Mapping) {
				if (ReverseMapping.ContainsKey(item.Value)) {
					throw new ArgumentException($"The string '{item.Value}' has already been added as a key (redefinition)!");
				}
				ReverseMapping.Add(item.Value, item.Key);
			}
		}

		private string langCode;

		public string Fullname { get; set; }

		public Dictionary<int, string> Mapping { get; }

		public Dictionary<string, int> ReverseMapping { get; }
	}
}