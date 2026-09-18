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

/// Pure deep-link policy, unit-tested in `test/unit/router_redirect_test.dart`.
///
/// - Unauthenticated users can only stay on `/login` and `/register`.
/// - Authenticated users are bounced off those public routes (and `/`)
///   to their role home: `/org` for org admins, `/profile` otherwise.
String? resolveAuthRedirect({
  required AuthState authState,
  required String location,
}) {
  const public = ['/login', '/register'];
  final user = switch (authState) {
    AuthAuthenticated(:final user) => user,
    _ => null,
  };

  if (user == null) {
    return public.contains(location) ? null : '/login';
  }
  if (location == '/' || public.contains(location)) {
    return user.role == AppRoles.orgAdmin ? '/org' : '/profile';
  }
  return null;
}

/// Central GoRouter. Unauthenticated users are redirected to `/login`;
/// authenticated users hitting `/login`/`/register` go to their role home.
GoRouter createRouter(AuthBloc authBloc) {
  return GoRouter(
    initialLocation: '/login',
    refreshListenable: AuthStateListenable(authBloc),
    redirect: (context, state) => resolveAuthRedirect(
      authState: authBloc.state,
      location: state.matchedLocation,
    ),
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
        redirect: (context, state) => resolveAuthRedirect(
          authState: authBloc.state,
          location: '/',
        ),
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
