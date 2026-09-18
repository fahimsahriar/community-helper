import 'package:bloc_test/bloc_test.dart';
import 'package:community_helper/core/error/api_exception.dart';
import 'package:community_helper/core/error/failure.dart';
import 'package:community_helper/features/auth/application/bloc/auth_bloc.dart';
import 'package:community_helper/features/auth/application/bloc/auth_event.dart';
import 'package:community_helper/features/auth/application/bloc/auth_state.dart';
import 'package:community_helper/features/auth/data/models/auth_response_model.dart';
import 'package:community_helper/features/auth/data/models/user_model.dart';
import 'package:community_helper/features/auth/domain/repositories/auth_repository.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';

class MockAuthRepository extends Mock implements AuthRepository {}

void main() {
  const user = UserModel(
    id: 'u1',
    email: 'vol@example.com',
    role: 'volunteer',
  );
  final authResponse = AuthResponseModel(
    accessToken: 'access',
    refreshToken: 'refresh',
    expiresAtUtc: DateTime.utc(2030),
    user: user,
  );

  late MockAuthRepository repository;

  setUpAll(() {
    registerFallbackValue('');
  });

  setUp(() {
    repository = MockAuthRepository();
  });

  group('AuthBloc', () {
    blocTest<AuthBloc, AuthState>(
      'emits [Loading, Authenticated] when login succeeds',
      build: () {
        when(
          () => repository.login(
            email: any(named: 'email'),
            password: any(named: 'password'),
          ),
        ).thenAnswer((_) async => authResponse);
        return AuthBloc(repository);
      },
      act: (bloc) => bloc.add(
        const AuthLoginRequested(email: 'vol@example.com', password: 'password123'),
      ),
      expect: () => [const AuthLoading(), const AuthAuthenticated(user)],
      verify: (_) {
        verify(
          () => repository.login(
            email: 'vol@example.com',
            password: 'password123',
          ),
        ).called(1);
      },
    );

    blocTest<AuthBloc, AuthState>(
      'emits [Loading, Failure] when login throws AuthException',
      build: () {
        when(
          () => repository.login(
            email: any(named: 'email'),
            password: any(named: 'password'),
          ),
        ).thenThrow(
          const AuthException(UnauthorizedFailure('Bad credentials.')),
        );
        return AuthBloc(repository);
      },
      act: (bloc) => bloc.add(
        const AuthLoginRequested(email: 'vol@example.com', password: 'wrongpass'),
      ),
      expect: () => [
        const AuthLoading(),
        const AuthFailure('Bad credentials.'),
      ],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [Loading, Authenticated] when register succeeds',
      build: () {
        when(
          () => repository.register(
            email: any(named: 'email'),
            password: any(named: 'password'),
            role: any(named: 'role'),
          ),
        ).thenAnswer((_) async => authResponse);
        return AuthBloc(repository);
      },
      act: (bloc) => bloc.add(
        const AuthRegisterRequested(
          email: 'new@example.com',
          password: 'password123',
          role: 'volunteer',
        ),
      ),
      expect: () => [const AuthLoading(), const AuthAuthenticated(user)],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [Unauthenticated] on logout',
      build: () {
        when(() => repository.logout()).thenAnswer((_) async {});
        return AuthBloc(repository);
      },
      act: (bloc) => bloc.add(const AuthLogoutRequested()),
      expect: () => [const AuthUnauthenticated()],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [Loading, Authenticated] when TokenRefreshed restores session',
      build: () {
        when(() => repository.refreshSession()).thenAnswer((_) async => true);
        when(() => repository.restoreSession())
            .thenAnswer((_) async => user);
        return AuthBloc(repository);
      },
      act: (bloc) => bloc.add(const AuthTokenRefreshed()),
      expect: () => [const AuthLoading(), const AuthAuthenticated(user)],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [Loading, Unauthenticated] when TokenRefreshed fails',
      build: () {
        when(() => repository.refreshSession()).thenAnswer((_) async => false);
        return AuthBloc(repository);
      },
      act: (bloc) => bloc.add(const AuthTokenRefreshed()),
      expect: () => [const AuthLoading(), const AuthUnauthenticated()],
    );
  });
}
