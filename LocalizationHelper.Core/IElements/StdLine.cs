using System;

namespace LocalizationHelper.IElements {
	public class StdLine : IElement {
		public StdLine(string line = null) {
			this.line = line ?? Environment.NewLine;
		}

		public StdLine(int id, string localizedText) {
			line = $"{id}:{localizedText}";
		}

		private readonly string line;

		public string GetStr() {
			return line;
		}
	}
}