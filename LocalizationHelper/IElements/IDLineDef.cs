namespace LocalizationHelper.IElements {
	public class IDLineDef : IElement {
		public IDLineDef(string name, int id) {
			line = $"\t\t\tpublic const int {name} = {id};";
			ID = id;
		}

		public IDLineDef(string line) {
			this.line = line;
			
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) return;
			ID = int.Parse(line.Split(':')[0]);
		}

		public int ID { get; }

		private readonly string line;

		public string GetStr() {
			return line;
		}
	}
}