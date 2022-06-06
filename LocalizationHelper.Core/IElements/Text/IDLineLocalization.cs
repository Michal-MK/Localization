using System.Collections.Generic;

namespace LocalizationHelper.Core.IElements.Text {
	public class IDLineLocalization : ILangElement {
		public IDLineLocalization(LangSection langSection, int id, string value) {
			ID = id;
			LangSection = langSection;
			Value = value;
		}

		public int ID { get; }
		public string Value { get; set; }
		public LangSection LangSection { get; }

		public string GetStr() {
			return ID + ":" + Value;
		}

		public List<IDLineLocalization> GetAllDefs() {
			return new List<IDLineLocalization>() { this };
		}
	}
}