using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizationHelper.Core.IElements.Text {
	public class LangSection : IElement {
		public LangSection(LangFile langFile, string comment) {
			Comment = comment;
			LangFile = langFile;
		}

		public static LangSection Parse(LangFile langFile, ref int i, string[] lines) {
			LangSection ret = new(langFile, lines[i]);

			i++;

			while (i < lines.Length && (!string.IsNullOrWhiteSpace(lines[i]))) {
				if (lines[i].StartsWith("#")) {
					i++;
					continue;
				}
				ret.Definitions.Add(new IDLineLocalization(ret, ret.Comment, int.Parse(lines[i].Split(":")[0])));
				i++;
			}
			return ret;
		}

		public LangFile LangFile { get; }
		public string Comment { get; }
		public List<IElement> Definitions { get; } = new();

		public IEnumerable<IDLineLocalization> FindAllDefinitions(string query) {
			return Definitions.Where(w => w.GetType() == typeof(IDLineLocalization))
							  .Cast<IDLineLocalization>()
							  .Where(w => w.GetStr().ToLower().Contains(query.ToLower()));
		}

		public string GetStr() {
			return Comment + Environment.NewLine +
				   string.Join(Environment.NewLine, Definitions.Select(s => s.GetStr())) +
				   Environment.NewLine;
		}
	}
}
