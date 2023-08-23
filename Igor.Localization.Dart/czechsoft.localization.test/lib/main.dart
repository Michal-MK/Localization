import 'package:fluent_ui/fluent_ui.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:igor.localization/czechsoft.localization.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return const FluentApp(
      home: MyHomePage(title: 'Igor.Localization Demo'),
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key, required this.title});

  final String title;

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  TextEditingController enController = TextEditingController(text: 'Are you sure you want to delete {r0:0-10||10-|all }{r0:1|this|2-|these}{0:1||2-10| \\0|10-|} entr{r0:1|y|2-|ies}{r0:0-10||10-| (\\0)}?');
  TextEditingController csController = TextEditingController(text: 'Opravdu chcete smazat {r0:0-10||10-|všechny }{r0:1|tento|2-5|tyto|5-10|těchto|10-|tyto}{0:1||2-10| \\0|10-|} záznam{r0:1||2-5|y|5-10|ů|10-|y}{r0:0-10||10-| (\\0)}?');

  @override
  Widget build(BuildContext context) {
    return NavigationView(
      appBar: NavigationAppBar(
        backgroundColor: FluentTheme.of(context).accentColor,
        leading: null,
        automaticallyImplyLeading: false,
        title: Text(widget.title, style: FluentTheme.of(context).typography.subtitle),
      ),
      content: BlocProvider(
        create: (context) => EditedTextCubit(
          EditedTextState(
            enController.text,
            csController.text,
            1,
          ),
        ),
        child: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const SizedBox(height: 32),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 32.0),
                child: BlocBuilder<EditedTextCubit, EditedTextState>(
                  buildWhen: (previous, current) => previous.csText != current.csText,
                  builder: (context, state) {
                    var cubit = context.read<EditedTextCubit>();
                    return TextBox(
                      controller: csController,
                      onChanged: cubit.newCsText,
                    );
                  },
                ),
              ),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 32.0),
                child: BlocBuilder<EditedTextCubit, EditedTextState>(
                  buildWhen: (previous, current) => previous.enText != current.enText,
                  builder: (context, state) {
                    var cubit = context.read<EditedTextCubit>();
                    return TextBox(
                      controller: enController,
                      onChanged: cubit.newEnText,
                    );
                  },
                ),
              ),
              const Spacer(flex: 2),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 32.0),
                child: BlocBuilder<EditedTextCubit, EditedTextState>(
                  buildWhen: (previous, current) => previous.sliderValue != current.sliderValue,
                  builder: (context, state) {
                    var cubit = context.read<EditedTextCubit>();
                    return Slider(value: state.sliderValue.toDouble(), min: 1, max: 15, onChanged: cubit.newSliderValue);
                  },
                ),
              ),
              const Spacer(flex: 1),
              BlocBuilder<EditedTextCubit, EditedTextState>(
                builder: (context, state) {
                  return Text(
                    '${state.translatedCsText}\n${state.translatedEnText}',
                  );
                },
              ),
              const Spacer(flex: 2),
            ],
          ),
        ),
      ),
    );
  }
}

class EditedTextCubit extends Cubit<EditedTextState> {
  EditedTextCubit(super.initialState);

  void newCsText(String text) {
    emit(EditedTextState(state.enText, text, state.sliderValue));
  }

  void newEnText(String text) {
    emit(EditedTextState(text, state.csText, state.sliderValue));
  }

  void newSliderValue(double value) {
    var rounded = value.round();
    emit(EditedTextState(state.enText, state.csText, rounded));
  }
}

class EditedTextState {
  final String csText;
  final String enText;
  final int sliderValue;

  String get translatedCsText {
    try {
      return Texts().smartFormat(csText, [sliderValue]);
    } on Exception catch (e) {
      return e.toString();
    }
  }

  String get translatedEnText {
    try {
      return Texts().smartFormat(enText, [sliderValue]);
    } on Exception catch (e) {
      return e.toString();
    }
  }

  EditedTextState(this.enText, this.csText, this.sliderValue);
}
