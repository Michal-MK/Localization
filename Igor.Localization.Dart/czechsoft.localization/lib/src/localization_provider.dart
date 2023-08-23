import 'package:flutter/widgets.dart';
import 'package:igor.localization/src/parsers/localization_component.dart';

class LocalizationProvider extends ChangeNotifier {
  static LocalizationProvider? _instance;

  static LocalizationProvider get instance {
    _instance ??= LocalizationProvider._();
    return _instance!;
  }
  
  LocalizationProvider._();
}
class Texts extends ChangeNotifier {

  String smartFormat(String text, List<Object> args) {
    List<Compartment> parts = _getCompartments(text);
    StringBuffer ret = StringBuffer();

    for (Compartment part in parts) {
      ret.write(part.isPlain ? part.str : part.process(parts, args));
    }
    return ret.toString().replaceAll("\\n", "\n");
  }

  List<Compartment> _getCompartments(String localizedString) {
    List<Compartment> compartments = [];

    int nesting = 0;

    StringBuffer builder = StringBuffer();

    for (String c in localizedString.characters) {
      if (c == '{') {
        if (nesting == 0) {
          compartments.add(Compartment(str: builder.toString()));
          builder.clear();
        } else {
          builder.write(c);
        }
        nesting++;
        continue;
      }
      if (c == '}') {
        if (nesting == 1) {
          compartments.add(Compartment(str: builder.toString(), isArgument: true));
          builder.clear();
        } else {
          builder.write(c);
        }
        nesting--;
        continue;
      }
      builder.write(c);
    }
    if (builder.length > 0) {
      compartments.add(Compartment(str: builder.toString()));
    }

    return compartments.toList();
  }
}
