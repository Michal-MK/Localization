using System;
using System.Collections.Generic;
using System.Text;

namespace Igor.Localization {
	/// <summary>
	/// The core class to access localization data from
	/// </summary>
	public class LocaleProvider {

		private const string MISSING = "MISSING ";

		private readonly Dictionary<string, Language> languageDictionary;

		private Language currentLanguage;
		private Language defaultLanguage;

		/// <summary>
		/// After calling <see cref="Initialize"/> this static field becomes initialized, use it to access localization
		/// </summary>
		public static LocaleProvider Instance { get; private set; }

		/// <summary>
		/// The currently active language code, <see langword="null"/> if the active language is not set
		/// </summary>
		public string CurrentLanguage => currentLanguage?.LanguageCode;

		/// <summary>
		/// The default language code, <see langword="null"/> if the default language is not set
		/// </summary>
		public string DefaultLanguage => defaultLanguage?.LanguageCode;

		/// <summary>
		/// A collection of all loaded language codes
		/// </summary>
		public IReadOnlyCollection<string> LoadedLanguages => new List<string>(languageDictionary.Keys);

		/// <summary>
		/// Event raised whenever the active language changes
		/// </summary>
		public event EventHandler OnLanguageChanged;

		/// <summary>
		/// Initialize the localization library by providing the data folder
		/// </summary>
		/// <param name="localeFolder">the folder containing all the localization files</param>
		/// <returns>An instance of the <see cref="LocaleProvider"/>, also accessible by <see cref="Instance"/></returns>
		public static LocaleProvider Initialize(string localeFolder) {
			return Instance ?? new LocaleProvider(localeFolder);
		}

		private LocaleProvider(string localeFolder) {
			Instance = this;
			languageDictionary = LocaleParser.ParseLangFiles(localeFolder);
		}

		/// <summary>
		/// Sets an active language, all localization requests will from now on be querying the provided language
		/// </summary>
		/// <param name="languageCode">the language code to set as active</param>
		/// <param name="setAsDefault">a flag to set the default language, should be used only once</param>
		public void SetActiveLanguage(string languageCode, bool setAsDefault = false) {
			languageCode = languageCode.Trim().ToLower();
			if (languageDictionary.ContainsKey(languageCode)) {
				currentLanguage = languageDictionary[languageCode];
				OnLanguageChanged?.Invoke(this, EventArgs.Empty);
				if (setAsDefault) {
					defaultLanguage = currentLanguage;
				}
			}
			else {
				throw new InvalidOperationException($"Language with code {languageCode} is not implemented!");
			}
		}

		/// <summary>
		/// The main simple get function
		/// </summary>
		/// <param name="stringCode">the code of the localized string</param>
		/// <param name="tryDefaultIfMissing">a flag to try obtaining the localized string from the default language in case the active language does not have the localized string defined</param>
		/// <returns>The localized string</returns>
		public string Get(int stringCode, bool tryDefaultIfMissing = false) {
			if (currentLanguage.Mapping.TryGetValue(stringCode, out string value)) {
				return value;
			}
			if (tryDefaultIfMissing && defaultLanguage != null && defaultLanguage.Mapping.TryGetValue(stringCode, out value)) {
				return value;
			}
			return MISSING + stringCode;
		}

		/// <summary>
		/// The main smart get function
		/// </summary>
		/// <param name="stringCode">the code of the localized string</param>
		/// <param name="args">optional parameters to the templated localization string </param>
		/// <returns>The localized string</returns>
		public string SmartFormat(int stringCode, params object[] args) {
			if (!currentLanguage.Mapping.TryGetValue(stringCode, out string localizedString)) {
				return MISSING + stringCode;
			}
			Compartment[] parts = GetCompartments(localizedString);
			StringBuilder ret = new StringBuilder();

			foreach (Compartment part in parts) {
				_ = ret.Append(part.Plain ? part.Str : part.Process(parts, args));
			}
			return ret.ToString().Replace("\\n", "\n");
		}

		/// <summary>
		/// The main simple get function just for the default language
		/// </summary>
		/// <exception cref="InvalidOperationException">if the default language is not set</exception>
		/// <param name="stringCode">the code of the localized string</param>
		/// <returns>The localized string</returns>
		public string GetFromDefault(int stringCode) {
			if (defaultLanguage == null) {
				throw new InvalidOperationException("Default language was not set!");
			}
			if (defaultLanguage.Mapping.TryGetValue(stringCode, out string value)) {
				return value;
			}
			return MISSING + stringCode;
		}

		/// <summary>
		/// Reverse mapping for localized strings
		/// </summary>
		/// <exception cref="InvalidOperationException">if an exact match for <see cref="text"/> does not exist</exception>
		/// <param name="text">the localized text</param>
		/// <returns>The code of the localized string</returns>
		public int GetCode(string text) {
			if (currentLanguage.ReverseMapping.TryGetValue(text, out int code)) {
				return code;
			}
			if (text == MISSING) {
				return int.MinValue;
			}
			throw new InvalidOperationException($"Exact match for {text} does not exist!");
		}

		/// <summary>
		/// Returns the full language name as specified in the localization file <see langword="null"/> if not set
		/// </summary>
		/// <param name="languageCode">the language code</param>
		/// <returns>The full language name</returns>
		public string GetFullName(string languageCode) {
			return languageDictionary[languageCode].Fullname;
		}

		private static Compartment[] GetCompartments(string localizedString) {
			List<Compartment> compartments = new List<Compartment>();

			int nesting = 0;

			StringBuilder builder = new StringBuilder();

			foreach (char c in localizedString) {
				if (c == '{') {
					if (nesting == 0) {
						compartments.Add(new Compartment(builder.ToString()));
						builder.Clear();
					}
					else {
						builder.Append(c);
					}
					nesting++;
					continue;
				}
				if (c == '}') {
					if (nesting == 1) {
						compartments.Add(new Compartment(builder.ToString()) { IsArgument = true });
						builder.Clear();
					}
					else {
						builder.Append(c);
					}
					nesting--;
					continue;
				}
				builder.Append(c);
			}
			if (builder.Length > 0) {
				compartments.Add(new Compartment(builder.ToString()));
			}

			return compartments.ToArray();
		}
	}
}
