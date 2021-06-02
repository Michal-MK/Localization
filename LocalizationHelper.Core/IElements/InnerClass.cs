using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizationHelper.Core.IElements {
	public class InnerClass : IElement {
		public InnerClass(string fileName, string name) {
			firstLine = "\t\tpublic class " + name + " {";
			Name = name;
			FileName = fileName;
			lastLine = "\t\t}";
		}

		public InnerClass(string fileName, ref int i, string[] lines) {
			firstLine = lines[i];
			Name = firstLine.Split(":")[0]
							.Trim()
							.Replace("public class ", "")
							.Replace(" {", "");
			i++;
			FileName = fileName;

			while (i < lines.Length && lines[i].Trim() != "}") {
				if (lines[i].TrimStart().StartsWith("public const int ")) {
					Internals.Add(new IDLineDef(fileName, Name, lines[i]));
				}
				else if (lines[i].TrimStart().StartsWith("public class ")) {
					Internals.Add(new InnerClass(fileName, ref i, lines));
				}
				else {
					Internals.Add(new StdLine(lines[i]));
				}
				i++;
			}
			lastLine = lines[i];
		}

		public string FileName { get; }
		public string Name { get; }
		public List<IElement> Internals { get; } = new();

		private readonly string firstLine;
		private readonly string lastLine;

		public IEnumerable<IDLineDef> FindAllDefinitions() {
			IEnumerable<IDLineDef> ret = new List<IDLineDef>();
			ret = Internals.Where(w => w.GetType() == typeof(IDLineDef))
						   .Cast<IDLineDef>().Concat(ret);

			ret = Internals.Where(w => w.GetType() == typeof(InnerClass))
						   .Cast<InnerClass>().SelectMany(s => s.FindAllDefinitions())
						   .Concat(ret);

			return ret;
		}

		public string GetStr() {
			return firstLine + Environment.NewLine +
				   string.Join(Environment.NewLine, Internals.Select(s => s.GetStr())) + Environment.NewLine +
				   lastLine;
		}

		public IEnumerable<InnerClass> GetAllInnerClasses() {
			return Internals.Where(w => w.GetType() == typeof(InnerClass))
							.Cast<InnerClass>()
							.SelectMany(s => s.GetAllInnerClasses()).Concat(new[] { this });
		}
	}
}
