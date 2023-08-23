class Compartment {
		//Example: Opravdu chcete smazat {r0:1|tento|2-5|tyto|5-|těchto}{0:1||2-| \0} záznam{r0:1||2-5|y|5-|ů}?

		/*To + Processing afterwards: 
		[
			Opravdu chcete smazat 
			r0:1|tento|2-|tyto
			 
			0:1||2-|\0
			 záznam
			r0:1||2-|y
			?
		]
		*/

		Compartment({required this.str, this.isArgument = false});

    String str;

		bool isArgument;
		bool get isPlain => !isArgument && !isBackReference().tf;

		List<String> get split => str.split(RegExp('[:|]'));

		String? processedString;

		bool get hasID => isArgument && split.isNotEmpty && int.tryParse(split[0]) != null;

		bool get hasBackRefID => isArgument && split.isNotEmpty && int.tryParse(split[0].substring(1)) != null;

		bool get hasRules => isArgument && (hasID || hasBackRefID) && split.length > 1 && str.contains(":");

		int get id => int.parse(split[0]);

		({bool tf, int? refID}) isBackReference() {
			int? refID = -1;
			if (str.length <= 1) return (tf: false, refID: refID);
			refID = int.tryParse(str[1].toString());

			return (tf: isArgument && str.length > 1 && str[0] == 'r' && refID != null, refID: refID);
		}

    @override
		String toString() {
			return str;
		}

		String process(List<Compartment> parts, List<Object> args) {
			if (processedString != null) return processedString!;

      var backReference = isBackReference();

			if (backReference.tf) {
				Compartment origin = findOrigin(backReference.refID!, parts);
				origin.process(parts, args);

				Object arg = args[backReference.refID!];
				for (int i = 1; i < split.length; i += 2) {
					LocalizationRule rule = LocalizationRule(split[i], split[i + 1]);
					if (rule.isMatch(arg.toString())) {
						return processedString = postProcess(rule.result, args);
					}
				}
				throw Exception("$str [$arg] < MATCH NOT EXHAUSTIVE");
			}
			if (isArgument) {
				if (hasRules) {
					Object arg = args[id];
					for (int i = 1; i < split.length; i += 2) {
						LocalizationRule rule = LocalizationRule(split[i], split[i + 1]);
						if (rule.isMatch(arg.toString())) {
							return processedString = postProcess(rule.result, args);
						}
					}
					throw Exception("$str [$arg] < MATCH NOT EXHAUSTIVE");
				}
				if (hasID) {
					return processedString = args[id].toString();
				}
				switch (str) {
					case "OB":
						return processedString = "{";
					case "CB":
						return processedString = "}";
				}
			}
			if (isPlain) {
				return processedString = postProcess(str, args);
			}

			throw Exception("UNKNOWN ERROR: $str");
		}

		static Compartment findOrigin(int refID, Iterable<Compartment> parts) {
			List<Compartment> cmp = parts.where((w) => w.hasID && w.id == refID)
										 .toList();
			if (cmp.length != 1) {
				throw Exception("REF ID NOT FOUND!");
			}
			return cmp[0];
		}

		static String postProcess(String text, List<Object> args) {
			for (int i = 0; i < args.length; i++) {
				text = text.replaceAll("\\$i", args[i].toString());
			}
			return text;
		}
	}

  class LocalizationRule {

    LocalizationRule(this.match, this.result);

		String match;
		String result;

		bool get isRange => match.contains("-");

		(int min, int max) getRange() {
			if (!isRange) {
				throw Exception("$match < NOT A RANGE");
			}
			List<String> split = match.split('-');
			if (split[0] == "") {
				return (-1 >>> 1, int.parse(split[1]));
			}
			else if (split[1] == "") {
				return (int.parse(split[0]), -1 >>> 1);
			}
			else {
				return (int.parse(split[0]), int.parse(split[1]));
			}
		}

		int getValue() {
			if (isRange) {
				throw Exception("$match < IS A RANGE");
			}
			return int.parse(match);
		}

		bool isMatch(String arg) {
			int argInt = int.parse(arg);
			if (isRange) {
				var (int min, int max) = getRange();
				return argInt >= min && argInt < max;
			}
			else {
				return argInt == getValue();
			}
		}
	}