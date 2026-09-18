import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import 'core/router/app_router.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/application/bloc/auth_bloc.dart';
import 'features/auth/application/bloc/auth_event.dart';
import 'injection.dart';

/// Root widget: provides the shared [AuthBloc] and GoRouter.
class CommunityHelperApp extends StatefulWidget {
  const CommunityHelperApp({super.key});

  @override
  State<CommunityHelperApp> createState() => _CommunityHelperAppState();
}

class _CommunityHelperAppState extends State<CommunityHelperApp> {
  late final AuthBloc _authBloc;
  late final GoRouter _router;

  @override
  void initState() {
    super.initState();
    _authBloc = sl<AuthBloc>()..add(const AuthStarted());
    _router = createRouter(_authBloc);
  }

  @override
  void dispose() {
    _authBloc.close();
    _router.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocProvider.value(
      value: _authBloc,
      child: MaterialApp.router(
        title: 'CommunityHelper',
        theme: AppTheme.light(),
        routerConfig: _router,
      ),
    );
  }
}
