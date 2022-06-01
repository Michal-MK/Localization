namespace LocalizationHelper.Core.IElements.Text {
	public class IDLineLocalization : IElement {
		public IDLineLocalization(LangSection langSection, string parentName, int id) {
			ID = id;
			Parent = parentName;
			LangSection = langSection;
		}

		public int ID { get; }
		public string Parent { get; }
		public LangSection LangSection { get; }

		public string GetStr() {
			return "TODO";
		}
	}
}
