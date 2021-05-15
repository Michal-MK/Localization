using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalizationHelper.IElements {
	public class LangSection : IElement {
		public LangSection() { }

		public LangSection(string comment) {
			Comment = comment;
		}

		public string Comment { get; set; }

		public List<IElement> Definitions { get; set; } = new();

		public static LangSection Parse(ref int i, string[] lines) {
			LangSection ret = new();

			ret.Comment = lines[i];
			i++;

			while (i < lines.Length && (!string.IsNullOrWhiteSpace(lines[i]) || i + 1 < lines.Length && !lines[i + 1].Trim().StartsWith('#'))) {
				ret.Definitions.Add(new IDDef(lines[i]));
				i++;
			}
			return ret;
		}

		public string GetStr() {
			return Comment + Environment.NewLine + string.Join(Environment.NewLine, Definitions.Select(s => s.GetStr())) + Environment.NewLine;
		}
	}
}
