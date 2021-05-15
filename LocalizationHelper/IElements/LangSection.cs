using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalizationHelper.IElements {
	public class LangSection : IElement {
		private LangSection() { }

		public LangSection(string comment) {
			Comment = comment;
		}

		public string Comment { get; private set; }

		public List<IElement> Definitions { get; } = new();

		public static LangSection Parse(ref int i, string[] lines) {
			LangSection ret = new() { Comment = lines[i] };
			i++;

			while (i < lines.Length && (!string.IsNullOrWhiteSpace(lines[i]) || i + 1 < lines.Length && !lines[i + 1].Trim().StartsWith('#'))) {
				ret.Definitions.Add(new IDLineDef(lines[i]));
				i++;
			}
			return ret;
		}

		public string GetStr() {
			return Comment + Environment.NewLine +
				   string.Join(Environment.NewLine, Definitions.Select(s => s.GetStr())) +
				   Environment.NewLine;
		}
	}
}