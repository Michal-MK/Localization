using System;
using System.Collections.Generic;

namespace Igor.Localization {
	internal class Language {

		internal Language(LocaleData data) {
			LanguageCode = data.LanguageCode;

			Fullname = data.FullLangName;
			Mapping = data.Language;
			ReverseMapping = new Dictionary<string, int>();
			RedefinitionMapping = new Dictionary<int, int>();
			foreach (KeyValuePair<int, string> item in Mapping) {
				if (ReverseMapping.ContainsKey(item.Value)) {
					RedefinitionMapping.Add(item.Key, ReverseMapping[item.Value]);
				}
				else {
					ReverseMapping.Add(item.Value, item.Key);
				}
			}
		}

		internal string LanguageCode { get; }

		internal string Fullname { get; set; }

		internal Dictionary<int, string> Mapping { get; }

		internal Dictionary<string, int> ReverseMapping { get; }

		private Dictionary<int, int> RedefinitionMapping { get; }

		public bool TryGetValue(int stringCode, out string localizedString) {
			localizedString = "";
			if (Mapping.ContainsKey(stringCode)) {
				localizedString = Mapping[stringCode];
				return true;
			}
			if (RedefinitionMapping.ContainsKey(stringCode)) {
				localizedString = Mapping[RedefinitionMapping[stringCode]];
				return true;
			}
			return false;
		}
	}
}