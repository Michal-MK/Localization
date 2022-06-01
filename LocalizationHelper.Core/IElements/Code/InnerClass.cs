using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizationHelper.Core.IElements.Code {
	public class InnerClass : IElement {
		public InnerClass(ClassFile classFile, string name) {
			firstLine = "\t\tpublic class " + name + " {";
			Name = name;
			ClassFile = classFile;
			lastLine = "\t\t}";
		}

		public InnerClass(ClassFile classFile, ref int i, string[] lines) {
			firstLine = lines[i];
			Name = firstLine.Split(":")[0]
							.Trim()
							.Replace("public class ", "")
							.Replace(" {", "");
			i++;
			ClassFile = classFile;

			while (i < lines.Length && lines[i].Trim() != "}") {
				if (lines[i].TrimStart().StartsWith("public const int ")) {
					Internals.Add(new IDLineDefinition(ClassFile, Name, lines[i]));
				}
				else if (lines[i].TrimStart().StartsWith("public class ")) {
					Internals.Add(new InnerClass(classFile, ref i, lines));
				}
				else {
					Internals.Add(new StdLine(lines[i]));
				}
				i++;
			}
			lastLine = lines[i];
		}

		public ClassFile ClassFile { get; }
		public string Name { get; }
		public List<IElement> Internals { get; } = new();

		private readonly string firstLine;
		private readonly string lastLine;

		public IEnumerable<IDLineDefinition> FindAllDefinitions() {
			IEnumerable<IDLineDefinition> ret = new List<IDLineDefinition>();
			ret = Internals.Where(w => w.GetType() == typeof(IDLineDefinition))
						   .Cast<IDLineDefinition>().Concat(ret);

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
