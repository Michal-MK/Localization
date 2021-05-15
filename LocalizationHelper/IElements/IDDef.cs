namespace LocalizationHelper.IElements {
	public class IDDef : IElement {
		public IDDef(string name, int id) {
			Line = $"\t\t\tpublic const int {name} = {id};";
		}
		
		public IDDef(string line) {
			Line = line;
		}

		public string Line { get; set; }

		public string GetStr() {
			return Line;
		}
	}
}
