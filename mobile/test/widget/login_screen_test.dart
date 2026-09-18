import 'package:bloc_test/bloc_test.dart';
import 'package:community_helper/features/auth/application/bloc/auth_bloc.dart';
import 'package:community_helper/features/auth/application/bloc/auth_event.dart';
import 'package:community_helper/features/auth/application/bloc/auth_state.dart';
import 'package:community_helper/features/auth/presentation/screens/login_screen.dart';
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

  Future<void> pumpLogin(WidgetTester tester) {
    return tester.pumpWidget(
      MaterialApp(
        home: BlocProvider<AuthBloc>.value(
          value: bloc,
          child: const LoginScreen(),
        ),
      ),
    );
  }

  testWidgets('renders email + password fields and login button', (tester) async {
    await pumpLogin(tester);

    expect(find.byKey(const Key('emailField')), findsOneWidget);
    expect(find.byKey(const Key('passwordField')), findsOneWidget);
    expect(find.text('Log in'), findsWidgets);
  });

  testWidgets('dispatches AuthLoginRequested on valid submit', (tester) async {
    when(() => bloc.state).thenReturn(const AuthInitial());

    await pumpLogin(tester);
    await tester.enterText(
      find.byKey(const Key('emailField')),
      'vol@example.com',
    );
    await tester.enterText(
      find.byKey(const Key('passwordField')),
      'password123',
    );
    await tester.tap(find.text('Log in').first);
    await tester.pump();

    verify(
      () => bloc.add(
        const AuthLoginRequested(
          email: 'vol@example.com',
          password: 'password123',
        ),
      ),
    ).called(1);
  });

  testWidgets('shows failure message on AuthFailure', (tester) async {
    whenListen(
      bloc,
      Stream.fromIterable([
        const AuthLoading(),
        const AuthFailure('Bad credentials.'),
      ]),
      initialState: const AuthInitial(),
    );

    await pumpLogin(tester);
    await tester.pump();

    expect(find.text('Bad credentials.'), findsWidgets);
  });
}
