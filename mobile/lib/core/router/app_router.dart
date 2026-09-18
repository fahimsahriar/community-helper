import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../features/auth/application/bloc/auth_bloc.dart';
import '../../features/auth/application/bloc/auth_state.dart';
import '../../features/auth/data/models/user_model.dart';
import '../../features/auth/presentation/screens/login_screen.dart';
import '../../features/auth/presentation/screens/register_screen.dart';
import '../../features/profile/application/cubit/profile_cubit.dart';
import '../../features/profile/presentation/screens/onboarding_screen.dart';
import '../../features/profile/presentation/screens/org_profile_screen.dart';
import '../../features/profile/presentation/screens/profile_screen.dart';
import '../../injection.dart';

/// Central GoRouter. Unauthenticated users are redirected to `/login`;
/// authenticated users hitting `/login`/`/register` go to `/`.
GoRouter createRouter(AuthBloc authBloc) {
  return GoRouter(
    initialLocation: '/login',
    refreshListenable: AuthStateListenable(authBloc),
    redirect: (context, state) {
      final authState = authBloc.state;
      final isAuthed = authState is AuthAuthenticated;
      final loc = state.matchedLocation;

      const public = ['/login', '/register'];
      if (!isAuthed && !public.contains(loc)) {
        return '/login';
      }
      if (isAuthed && public.contains(loc)) {
        final user = (authState as AuthAuthenticated).user;
        return user.role == AppRoles.orgAdmin ? '/org' : '/profile';
      }
      return null;
    },
    routes: [
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginScreen(),
      ),
      GoRoute(
        path: '/register',
        builder: (context, state) => const RegisterScreen(),
      ),
      GoRoute(
        path: '/',
        redirect: (context, state) {
          final s = authBloc.state;
          if (s is AuthAuthenticated && s.user.role == AppRoles.orgAdmin) {
            return '/org';
          }
          return '/profile';
        },
        builder: (context, state) => const SizedBox.shrink(),
      ),
      GoRoute(
        path: '/onboarding',
        builder: (context, state) => BlocProvider(
          create: (_) => sl<ProfileCubit>(),
          child: const OnboardingScreen(),
        ),
      ),
      GoRoute(
        path: '/profile',
        builder: (context, state) => BlocProvider(
          create: (_) => sl<ProfileCubit>(),
          child: const ProfileScreen(),
        ),
      ),
      GoRoute(
        path: '/org',
        builder: (context, state) => BlocProvider(
          create: (_) => sl<ProfileCubit>(),
          child: const OrgProfileScreen(),
        ),
      ),
    ],
  );
}

/// Bridges AuthBloc state changes into GoRouter's `refreshListenable`.
class AuthStateListenable extends ChangeNotifier {
  AuthStateListenable(this._bloc) {
    _sub = _bloc.stream.listen((_) => notifyListeners());
  }

  final AuthBloc _bloc;
  late final StreamSubscription<AuthState> _sub;

  @override
  void dispose() {
    unawaited(_sub.cancel());
    super.dispose();
  }
}
