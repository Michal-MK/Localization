using LocalizationHelper.Core.IElements.Code;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace LocalizationHelper.Core.IElements.Text {
	public class LangFile : ILangElement {
		public static LangFile Parse(string langFilePath) {
			LangFile ret = new() { FileName = langFilePath };

			string[] lines = File.ReadAllLines(langFilePath);

			int start = 0;
			if (lines[0].StartsWith("/")) {
				ret.header = lines[0];
				start = 1;
			}

			for (int i = start; i < lines.Length; i++) {
				if (string.IsNullOrWhiteSpace(lines[i])) {
					ret.Sections.Add(new StdLine(lines[i]));
				}
				else if (lines[i].TrimStart().StartsWith("#")) {
					ret.Sections.Add(LangSection.Parse(ret, ref i, lines));
				}
			}

			return ret;
		}

		public List<ILangElement> Sections { get; } = new();

		public string FileName { get; init; }

		private string header;

		public IEnumerable<IDLineLocalization> FindAll(string query) {
			return Sections.OfType<LangSection>()
						   .SelectMany(s => s.FindAllDefinitions(query));
		}

		public string GetStr() {
			return (header != null ? header + Environment.NewLine : "") +
				   string.Join(Environment.NewLine, Sections.Select(s => s.GetStr()));
		}

		public List<IDLineLocalization> GetAllDefs() {
			return Sections.SelectMany(s => s.GetAllDefs()).ToList();
		}

		public LangSection? GetLangSection(InnerClass inner) {
			string header = inner.Name;
			return Sections.OfType<LangSection>().SingleOrDefault(s => s.Comment == "#" + header);
		}
	}
}