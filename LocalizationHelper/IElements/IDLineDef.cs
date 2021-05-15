namespace LocalizationHelper.IElements {
	public class IDLineDef : IElement {
		public IDLineDef(string name, int id) {
			line = $"\t\t\tpublic const int {name} = {id};";
		}

		public IDLineDef(string line) {
			this.line = line;
		}

		private readonly string line;

		public string GetStr() {
			return line;
		}
	}
}