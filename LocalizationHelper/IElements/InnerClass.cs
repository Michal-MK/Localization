using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalizationHelper.IElements {
	public class InnerClass : IElement {
		public InnerClass(string name) {
			FirstLine = "\t\tpublic class " + name + " {";
			Name = name;
			LastLine = "\t\t}";
		}

		public InnerClass(ref int i, string[] lines) {
			FirstLine = lines[i];
			Name = FirstLine.Trim().Replace("public class ", "").Replace(" {", "");
			i++;

			while (i < lines.Length && lines[i].Trim() != "}") {
				if (lines[i].TrimStart().StartsWith("public const int")) {
					Internals.Add(new IDDef(lines[i]));
				}
				else if (lines[i].TrimStart().StartsWith("public class ")) {
					Internals.Add(new InnerClass(ref i, lines));
				}
				else {
					Internals.Add(new StdLine(lines[i]));
				}
				i++;
			}

			LastLine = lines[i];
		}

		public string FirstLine { get; set; }

		public string Name { get; set; }

		public string LastLine { get; set; }

		public List<IElement> Internals { get; set; } = new();

		public string GetStr() {
			return FirstLine + Environment.NewLine + string.Join(Environment.NewLine, Internals.Select(s => s.GetStr())) + Environment.NewLine + LastLine;
		}
	}
}
