using LocalizationHelper.Core.IElements.Code;
using LocalizationHelper.Core.IElements.Text;
using System.Collections.Generic;

namespace LocalizationHelper.Core.IElements {

	public interface IElement {
	
	}

	public interface ICodeElement : IElement {
		string GetStr();

		List<IDLineDefinition> GetAllDefs();
	}

	public interface ILangElement : IElement {
		string GetStr();

		List<IDLineLocalization> GetAllDefs();
	}
}
