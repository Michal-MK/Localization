using LocalizationHelper.Core.IElements.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizationHelper.Core.IElements.Code {
	public class InnerClass : ICodeElement {
		public InnerClass(ClassFile classFile, string name, InnerClass? parent = null) {
			firstLine = (parent is not null ? parent.Indent : classFile.Indent) + "public class " + name + " {";
			Name = name;
			ClassFile = classFile;
			lastLine = classFile.Indent + "}";
			Parent = parent;
		}

		public int IndentLevel => Parent is not null ? Parent.IndentLevel + 1 : ClassFile.IndentLevel;
		public string Indent => new('\t', IndentLevel);

		public InnerClass(ClassFile classFile, ref int i, string[] lines, InnerClass? parent = null) {
			firstLine = lines[i];
			Name = firstLine.Split(":")[0]
				.Trim()
				.Replace("public class ", "")
				.Replace(" {", "");
			i++;
			ClassFile = classFile;
			Parent = parent;

			while (i < lines.Length && lines[i].Trim() != "}") {
				if (lines[i].TrimStart().StartsWith("public const int ")) {
					Internals.Add(new IDLineDefinition(ClassFile, lines[i], this));
				}
				else if (lines[i].TrimStart().StartsWith("public class ")) {
					Internals.Add(new InnerClass(classFile, ref i, lines, this));
				}
				else {
					Internals.Add(new StdLine(lines[i]));
				}
				i++;
			}
			lastLine = lines[i];
		}

		public ClassFile ClassFile { get; }

		public InnerClass? Parent { get; }
		public string Name { get; }
		public List<ICodeElement> Internals { get; } = new();

		private readonly string firstLine;
		private readonly string lastLine;

		public IEnumerable<IDLineDefinition> FindAllDefinitions() {
			IEnumerable<IDLineDefinition> ret = new List<IDLineDefinition>();
			ret = Internals.OfType<IDLineDefinition>().Concat(ret);

			ret = Internals.OfType<InnerClass>().SelectMany(s => s.FindAllDefinitions())
				.Concat(ret);

			return ret;
		}

		public string GetStr() {
			return firstLine + Environment.NewLine +
				   string.Join(Environment.NewLine, Internals.Select(s => s.GetStr())) + Environment.NewLine +
				   lastLine;
		}

		public IEnumerable<InnerClass> GetAllInnerClasses() {
			return Internals.OfType<InnerClass>()
				.SelectMany(s => s.GetAllInnerClasses()).Concat(new[] { this });
		}

		public List<IDLineDefinition> GetAllDefs() {
			return Internals.SelectMany(s => s.GetAllDefs()).ToList();
		}

		public string GetFullyQualifiedName() {
			return Parent is not null ? Parent.GetFullyQualifiedName() + "." + Name : Name;
		}
	}
}