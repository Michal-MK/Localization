using LocalizationHelper.IElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalizationHelper {
	public class LangFile : IElement {
		public static LangFile Parse(string langFilePath) {
			LangFile ret = new();

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
					ret.Sections.Add(LangSection.Parse(langFilePath, ref i, lines));
				}
			}

			return ret;
		}

		public List<IElement> Sections { get; } = new();

		private string header;

		public IEnumerable<IDLineDef> FindAll(string query) {
			return Sections.Where(w => w.GetType() == typeof(LangSection))
						   .Cast<LangSection>()
						   .SelectMany(s => s.FindAllDefinitions(query));
		}

		public string GetStr() {
			return (header != null ? header + Environment.NewLine : "") +
				   string.Join(Environment.NewLine, Sections.Select(s => s.GetStr()));
		}
	}
}
