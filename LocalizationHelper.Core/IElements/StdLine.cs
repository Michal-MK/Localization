using System;

namespace LocalizationHelper.Core.IElements {
	public class StdLine : IElement {
		public StdLine(string line = null) {
			Line = line ?? Environment.NewLine;
		}

		public string Line { get; }

		public string GetStr() {
			return Line;
		}
	}
}