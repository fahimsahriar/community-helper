import 'package:community_helper/core/router/app_router.dart';
import 'package:community_helper/features/auth/application/bloc/auth_state.dart';
import 'package:community_helper/features/auth/data/models/user_model.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  const volunteer = UserModel(
    id: 'u1',
    email: 'vol@example.com',
    role: 'volunteer',
  );
  const orgAdmin = UserModel(
    id: 'u2',
    email: 'org@example.com',
    role: 'org_admin',
  );

  group('resolveAuthRedirect', () {
    test('sends unauthenticated deep links to /login', () {
      expect(
        resolveAuthRedirect(
          authState: const AuthUnauthenticated(),
          location: '/profile',
        ),
        '/login',
      );
      expect(
        resolveAuthRedirect(
          authState: const AuthInitial(),
          location: '/org',
        ),
        '/login',
      );
    });

    test('keeps unauthenticated users on public routes', () {
      expect(
        resolveAuthRedirect(
          authState: const AuthUnauthenticated(),
          location: '/login',
        ),
        isNull,
      );
      expect(
        resolveAuthRedirect(
          authState: const AuthUnauthenticated(),
          location: '/register',
        ),
        isNull,
      );
    });

    test('bounces authenticated volunteers off public routes to /profile', () {
      expect(
        resolveAuthRedirect(
          authState: const AuthAuthenticated(volunteer),
          location: '/login',
        ),
        '/profile',
      );
      expect(
        resolveAuthRedirect(
          authState: const AuthAuthenticated(volunteer),
          location: '/',
        ),
        '/profile',
      );
    });

    test('bounces authenticated org admins to /org', () {
      expect(
        resolveAuthRedirect(
          authState: const AuthAuthenticated(orgAdmin),
          location: '/register',
        ),
        '/org',
      );
      expect(
        resolveAuthRedirect(
          authState: const AuthAuthenticated(orgAdmin),
          location: '/',
        ),
        '/org',
      );
    });

    test('leaves authenticated users on protected routes', () {
      expect(
        resolveAuthRedirect(
          authState: const AuthAuthenticated(volunteer),
          location: '/profile',
        ),
        isNull,
      );
      expect(
        resolveAuthRedirect(
          authState: const AuthAuthenticated(orgAdmin),
          location: '/org',
        ),
        isNull,
      );
    });
  });
}
