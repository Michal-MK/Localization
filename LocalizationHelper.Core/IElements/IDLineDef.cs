namespace LocalizationHelper.IElements {
	public class IDLineDef : IElement {
		public IDLineDef(string classFileFilePath, string parentName, string name, int id) {
			line = $"\t\t\tpublic const int {name} = {id};";
			ID = id;
			Parent = parentName;
			FileName = classFileFilePath;
		}

		public IDLineDef(string fileName, string parentName, string line) {
			this.line = line;
			Parent = parentName;
			FileName = fileName;
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
		public string FileName { get; }
		
		private readonly string line;
		
		public string GetStr() {
			return line;
		}
	}
}
