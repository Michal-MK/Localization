using System.Collections.Generic;

namespace LocalizationHelper.Core.IElements.Code {
	public class IDLineDefinition : ICodeElement {
		public IDLineDefinition(ClassFile classFile, string name, int id, InnerClass? parent = null) {
			ID = id;
			Name = name;
			ClassFile = classFile;
			Parent = parent;
		}

		public IDLineDefinition(ClassFile classFile, string line, InnerClass? parent = null) {
			ClassFile = classFile;
			Parent = parent;
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) return;
			if (line.Trim().StartsWith("public const ")) {
				string[] split = line.Split('=');
				if (int.TryParse(split[1].Replace(";", "").Trim(), out int id)) {
					ID = id;
				}
				else {
					IDString = split[1].Replace(";", "").Trim();
				}
				Name = split[0].Replace("public const int", "").Trim();
			}
		}

		public string? IDString { get; }
		public int? ID { get; }

		public string IDFinal => ID?.ToString() ?? IDString;
		public string Name { get; set; }
		public ClassFile ClassFile { get; }
		public InnerClass? Parent { get; }

		private string Line => (Parent is not null ? Parent.Indent : ClassFile.Indent) + $"public const int {Name} = {IDFinal};";

		public string GetStr() {
			return Line;
		}

		public List<IDLineDefinition> GetAllDefs() {
			return new List<IDLineDefinition> { this };
		}

		public string GetFullyQualifiedName() {
			return Parent is not null ? Parent.GetFullyQualifiedName() + "." + Name : "ERROR";
		}
	}
}