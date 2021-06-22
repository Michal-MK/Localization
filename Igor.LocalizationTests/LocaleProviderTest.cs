using System;
using System.Linq;
using System.Reflection;
using Igor.Localization;
using NUnit.Framework;

namespace Igor.LocalizationTests {
	[TestFixture]
	public class LocaleProviderTest {

		private LocaleProvider lp;

		[SetUp]
		public void Setup() {
			lp = LocaleProvider.Initialize("Locales/locale");
			lp.SetActiveLanguage("en-us", true);
		}

		[Test]
		public void InitNoLanguageSelected() {
			_ = LocaleProvider.Instance.GetType()
						  .GetProperty(nameof(LocaleProvider.Instance), BindingFlags.Public | BindingFlags.Static)
						  ?.SetMethod.Invoke(LocaleProvider.Instance, new object[] { null });

			LocaleProvider lpLocal = LocaleProvider.Initialize("Locales/locale");

			Assert.That(lpLocal.CurrentLanguage == null);
		}

		[Test]
		public void PostInitSelectedLanguage() {
			Assert.That(lp.CurrentLanguage == "en-us");
		}

		[Test]
		public void PostInitSelectedLanguageName() {
			Assert.That(lp.GetFullName("en-us") == "English");
		}

		[Test]
		public void LoadedLanguagesCollection() {
			Assert.That(lp.LoadedLanguages.Contains("cs-cz"));
			Assert.That(lp.LoadedLanguages.Contains("en-us"));
			Assert.That(lp.LoadedLanguages.Count() == 2);
		}

		[Test]
		public void PostInitSelectedDefaultLanguage() {
			Assert.That(lp.DefaultLanguage == "en-us");
		}

		[Test]
		public void PostInitSwitchLanguage() {
			lp.OnLanguageChanged += OnLanguageChanged;

			lp.SetActiveLanguage("cs-cz");

			lp.OnLanguageChanged -= OnLanguageChanged;

			Assert.That(lp.DefaultLanguage == "en-us");
			Assert.That(lp.CurrentLanguage == "cs-cz");

			void OnLanguageChanged(object sender, EventArgs e) {
				Assert.That(lp.CurrentLanguage == "cs-cz");
			}
		}

		[Test]
		public void PostInitSwitchNonExistentLanguage() {
			Assert.Throws<InvalidOperationException>(() => { lp.SetActiveLanguage("aa-bb"); });
		}

		[Test]
		public void GetLocalizationMissing() {
			Assert.That(lp.Get(-9999) == "MISSING -9999");
		}

		[Test]
		public void GetLocalization() {
			Assert.That(lp.Get(-1) == "Back");
		}

		[Test]
		public void GetLocalizationCode() {
			Assert.That(lp.GetCode("Back") == -1);
		}

		[Test]
		public void GetLocalizationFromDefault() {
			lp.SetActiveLanguage("cs-cz", true);
			lp.SetActiveLanguage("en-us");

			Assert.That(lp.GetFromDefault(-1) == "Zpět");
		}

		[Test]
		public void SmartFormatSimple() {
			// 40375312:No account for '{0}' exists!

			const string EXPECTED = "No account for 'Test' exists!";
			const int ID = 40375312;

			Assert.That(lp.SmartFormat(ID, "Test") == EXPECTED);
		}

		[Test]
		public void SmartFormatTwiceSameReference() {
			// 1:Hello {0}, how is it going {r0}!

			const string EXPECTED = "Hello Test, how is it going Test!";
			const int ID = 1;

			Assert.That(lp.SmartFormat(ID, "Test") == EXPECTED);
		}

		[Test]
		public void SmartFormatNumbering() {
			// 4:{0} element{r0:1||2-|s}!

			const string EXPECTED_ONE = "1 element!";
			const string EXPECTED_TWO = "2 elements!";
			const int ID = 4;

			Assert.That(lp.SmartFormat(ID, 1) == EXPECTED_ONE);
			Assert.That(lp.SmartFormat(ID, 2) == EXPECTED_TWO);
		}

		[Test]
		public void SmartFormatTwoRefs() {
			// 5:{0} - {1}

			const string EXPECTED = "1 - 2";
			const int ID = 5;

			Assert.That(lp.SmartFormat(ID, 1, 2) == EXPECTED);
		}

		[Test]
		public void SmartFormatTwoRefsNumberingOfTwo() {
			// 6:{0} - {1} element{r1:1||2-|s}!

			const string EXPECTED_ONE = "0 - 1 element!";
			const string EXPECTED_TWO = "-1 - 2 elements!";
			const int ID = 6;

			Assert.That(lp.SmartFormat(ID, 0, 1) == EXPECTED_ONE);
			Assert.That(lp.SmartFormat(ID, -1, 2) == EXPECTED_TWO);
		}

		[Test]
		public void TextRedefinition() {
			lp = LocaleProvider.Initialize("Locales/Redefinitions");
			lp.SetActiveLanguage("cs-cz", true);

			Assert.That(lp.Get(1) == "Zpět");
			Assert.That(lp.Get(2) == "Zpět");

			lp.SetActiveLanguage("en-us");

			Assert.That(lp.Get(1) == "Back");
			Assert.That(lp.Get(2) == "Abort");
		}
	}
}