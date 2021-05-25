using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizationHelper.Core.IElements {
	public class LangSection : IElement {
		public LangSection(string fileName, string comment) {
			Comment = comment;
			FileName = fileName;
		}
		
		public static LangSection Parse(string fileName, ref int i, string[] lines) {
			LangSection ret = new(fileName, lines[i]);

			i++;

			while (i < lines.Length && (!string.IsNullOrWhiteSpace(lines[i]) || i + 1 < lines.Length && !lines[i + 1].Trim().StartsWith('#'))) {
				ret.Definitions.Add(new IDLineDef(fileName, ret.Comment, lines[i]));
				i++;
			}
			return ret;
		}

		public string FileName { get; }
		public string Comment { get; }
		public List<IElement> Definitions { get; } = new();

		public IEnumerable<IDLineDef> FindAllDefinitions(string query) {
			return Definitions.Where(w => w.GetType() == typeof(IDLineDef))
							  .Cast<IDLineDef>()
							  .Where(w => w.GetStr().ToLower().Contains(query.ToLower()));
		}

		public string GetStr() {
			return Comment + Environment.NewLine +
				   string.Join(Environment.NewLine, Definitions.Select(s => s.GetStr())) +
				   Environment.NewLine;
		}
	}
}
