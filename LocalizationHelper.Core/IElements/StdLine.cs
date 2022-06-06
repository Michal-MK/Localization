using LocalizationHelper.Core.IElements.Code;
using LocalizationHelper.Core.IElements.Text;
using System;
using System.Collections.Generic;

namespace LocalizationHelper.Core.IElements {
	public class StdLine : ICodeElement, ILangElement {
		public StdLine(string line = null) {
			Line = line ?? Environment.NewLine;
		}

		public string Line { get; }



		public string GetStr() {
			return Line;
		}

		List<IDLineLocalization> ILangElement.GetAllDefs() {
			return new List<IDLineLocalization>();
		}
		List<IDLineDefinition> ICodeElement.GetAllDefs() {
			return new List<IDLineDefinition>();
		}
	}
}