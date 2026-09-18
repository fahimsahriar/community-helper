import 'package:bloc_test/bloc_test.dart';
import 'package:community_helper/features/auth/application/bloc/auth_bloc.dart';
import 'package:community_helper/features/auth/application/bloc/auth_event.dart';
import 'package:community_helper/features/auth/application/bloc/auth_state.dart';
import 'package:community_helper/features/auth/data/models/user_model.dart';
import 'package:community_helper/features/auth/presentation/screens/register_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';

class MockAuthBloc extends MockBloc<AuthEvent, AuthState> implements AuthBloc {}

void main() {
  late MockAuthBloc bloc;

  setUp(() {
    bloc = MockAuthBloc();
    when(() => bloc.state).thenReturn(const AuthInitial());
    when(() => bloc.stream).thenAnswer((_) => const Stream<AuthState>.empty());
  });

  Future<void> pumpRegister(WidgetTester tester) {
    return tester.pumpWidget(
      MaterialApp(
        home: BlocProvider<AuthBloc>.value(
          value: bloc,
          child: const RegisterScreen(),
        ),
      ),
    );
  }

  testWidgets('renders role picker with Volunteer selected by default',
      (tester) async {
    await pumpRegister(tester);

    expect(find.byKey(const Key('rolePicker')), findsOneWidget);
    expect(find.text('Volunteer'), findsWidgets);
    expect(find.text('Organization'), findsWidgets);
  });

  testWidgets('dispatches AuthRegisterRequested with volunteer role',
      (tester) async {
    await pumpRegister(tester);
    await tester.enterText(
      find.byKey(const Key('emailField')),
      'new@example.com',
    );
    await tester.enterText(
      find.byKey(const Key('passwordField')),
      'password123',
    );
    await tester.tap(find.text('Create account'));
    await tester.pump();

    verify(
      () => bloc.add(
        const AuthRegisterRequested(
          email: 'new@example.com',
          password: 'password123',
          role: AppRoles.volunteer,
        ),
      ),
    ).called(1);
  });
}
