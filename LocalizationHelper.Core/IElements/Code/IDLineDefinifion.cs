namespace LocalizationHelper.Core.IElements.Code {
	public class IDLineDefinition : IElement {
		public IDLineDefinition(ClassFile classFile, string parentName, string name, int id) {
			line = $"\t\t\tpublic const int {name} = {id};";
			ID = id;
			Parent = parentName;
			ClassFile = classFile;
		}

		public IDLineDefinition(ClassFile classFile, string parentName, string line) {
			this.line = line;
			Parent = parentName;
			ClassFile = classFile;
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) return;
			if (line.Trim().StartsWith("public const ")) {
				if (int.TryParse(line.Split('=')[1].Replace(";", "").Trim(), out int id)) {
					ID = id;
				}
			}
			else {
				ID = int.Parse(line.Split(':')[0]);
			}
		}

		public int ID { get; }
		public string Parent { get; }
		public ClassFile ClassFile { get; }

		private readonly string line;

		public string GetStr() {
			return line;
		}
	}
}
