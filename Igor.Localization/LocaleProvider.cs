using System;
using System.Collections.Generic;
using System.Text;

namespace Igor.Localization {
	public class LocaleProvider {

		private const string MISSING = "MISSING ";
		private const int MISSING_L = 8;

		public static LocaleProvider Instance { get; private set; }

		public event EventHandler OnLanguageChanged;

		private readonly Dictionary<string, Language> languageDictionary;

		private Language currentLanguage;
		private Language defaultLanguage;

		public string CurrentLanguage { get; private set; }

		public string DefaultLanguage { get; private set; }

		public static LocaleProvider Initialize(string localeFolder) {
			return Instance ?? new LocaleProvider(localeFolder);
		}


		public void SetActiveLanguage(string languageCode, bool setAsDefault = false) {
			languageCode = languageCode.Trim().ToLower();
			if (languageDictionary.ContainsKey(languageCode)) {
				currentLanguage = languageDictionary[languageCode];
				CurrentLanguage = languageCode;
				OnLanguageChanged?.Invoke(this, EventArgs.Empty);
				if (!setAsDefault) return;
				
				DefaultLanguage = languageCode;
				defaultLanguage = currentLanguage;
			}
			else {
				throw new InvalidOperationException($"Language with code {languageCode} is not implemented!");
			}
		}

		private LocaleProvider(string localeFolder) {
			Instance = this;
			languageDictionary = LocaleParser.ParseLangFiles(localeFolder);
		}

		public string Get(int stringCode, bool tryDefaultIfMissing = false) {
			if (currentLanguage.Mapping.TryGetValue(stringCode, out string value)) {
				return value;
			}
			if (tryDefaultIfMissing && defaultLanguage != null && defaultLanguage.Mapping.TryGetValue(stringCode, out value)) {
				return value;
			}
			return MISSING + stringCode;
		}

		public string SmartFormat(int stringCode, params object[] args) {
			if (!currentLanguage.Mapping.TryGetValue(stringCode, out string value)) {
				return MISSING + stringCode;
			}
			Compartment[] parts = GetCompartments(value);
			StringBuilder ret = new StringBuilder();

			foreach (Compartment part in parts) {
				ret.Append(part.Plain ? part.Str : part.Process(parts, args));
			}
			return ret.ToString().Replace("\\n","\n");
		}

		private static Compartment[] GetCompartments(string value) {
			List<Compartment> ret = new List<Compartment>();

			int nesting = 0;

			StringBuilder sb = new StringBuilder();

			foreach (char c in value) {
				if (c == '{') {
					if (nesting == 0) {
						ret.Add(new Compartment(sb.ToString()));
						sb.Clear();
					}
					else {
						sb.Append(c);
					}
					nesting++;
					continue;
				}
				if (c == '}') {
					if (nesting == 1) {
						ret.Add(new Compartment(sb.ToString()) { IsArgument = true });
						sb.Clear();
					}
					else {
						sb.Append(c);
					}
					nesting--;
					continue;
				}
				sb.Append(c);
			}
			if (sb.Length > 0) ret.Add(new Compartment(sb.ToString()));

			return ret.ToArray();
		}

		public string GetFromDefault(int stringCode) {
			if (defaultLanguage == null) {
				throw new InvalidOperationException("Default language was not set!");
			}
			if (defaultLanguage.Mapping.TryGetValue(stringCode, out string value)) {
				return value;
			}
			return MISSING + stringCode;
		}

		public int GetCode(string text) {
			if (currentLanguage.ReverseMapping.TryGetValue(text, out int code)) {
				return code;
			}
			if (text.StartsWith(MISSING)) {
				return int.Parse(text.Substring(MISSING_L));
			}
			throw new InvalidOperationException($"Exact match for {text} does not exist!");
		}

		public string GetFullName(string language) {
			return languageDictionary[language].Fullname;
		}
	}
}
