using System;

namespace LocalizationHelper.IElements {
	public class StdLine : IElement {

		public StdLine(string line = null) {
			if (line == null) {
				Line = Environment.NewLine;
			}
			else {
				Line = line;
			}
		}

		public StdLine(int ID, string localizedText) {
			Line = $"{ID}:{localizedText}";
		}

		public string Line { get; set; }

		public string GetStr() {
			return Line;
		}
	}
}
