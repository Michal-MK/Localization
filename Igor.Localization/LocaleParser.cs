using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Igor.Localization {
	internal static class LocaleParser {

		private static LocaleData ParseLangFile(string filePath) {
			try {
				string completeName = Path.GetFileNameWithoutExtension(filePath);
				int first = completeName.IndexOf('-', 0);
				int second = completeName.IndexOf('-', first + 1);
				if (second == -1) second = completeName.Length;
				string mainLangCode = completeName.Substring(0, second);
				LocaleData data = new LocaleData {
					Language = new Dictionary<int, string>(),
					LanguageCode = mainLangCode
				};

				using (StreamReader reader = File.OpenText(filePath)) {
					Regex regex = new Regex(@"(-?\d+):(.+)");
					while (!reader.EndOfStream) {
						string line = reader.ReadLine();
						if (line.StartsWith("#") || string.IsNullOrEmpty(line)) {
							continue;
						}
						if (line.StartsWith("/")) {
							data.FullLangName = line.Substring(1);
							continue;
						}
						Match match = regex.Match(line);
						data.Language.Add(int.Parse(match.Groups[1].Value), match.Groups[2].Value);
					}
				}
				return data;
			}
			catch {
				return null;
			}
		}

		internal static Dictionary<string, Language> ParseLangFiles(string localeFolder) {
			Dictionary<string, Language> ret = new Dictionary<string, Language>();

			FileInfo[] files = new DirectoryInfo(localeFolder).GetFiles("*.txt");
			foreach (FileInfo file in files) {
				LocaleData data = ParseLangFile(file.FullName);
				if (data == null) {
					continue;
				}
				if (ret.ContainsKey(data.LanguageCode)) {
					foreach (KeyValuePair<int, string> item in data.Language) {
						ret[data.LanguageCode].Mapping.Add(item.Key, item.Value);
						if (ret[data.LanguageCode].ReverseMapping.ContainsKey(item.Value)) {
							continue;
						}
						ret[data.LanguageCode].ReverseMapping.Add(item.Value, item.Key);
					}
					if (data.FullLangName != null) {
						ret[data.LanguageCode].Fullname = data.FullLangName;
					}
				}
				else {
					ret.Add(data.LanguageCode, new Language(data));
				}
			}
			return ret;
		}
	}
}
