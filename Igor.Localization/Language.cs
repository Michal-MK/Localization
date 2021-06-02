using System;
using System.Collections.Generic;

namespace Igor.Localization {
	internal class Language {

		internal Language(LocaleData data) {
			LanguageCode = data.LanguageCode;

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

		internal string LanguageCode { get; }

		internal string Fullname { get; set; }

		internal Dictionary<int, string> Mapping { get; }

		internal Dictionary<string, int> ReverseMapping { get; }
	}
}