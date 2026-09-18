import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../../../shared/widgets/error_view.dart';
import '../../../../shared/widgets/primary_button.dart';
import '../../application/bloc/auth_bloc.dart';
import '../../application/bloc/auth_event.dart';
import '../../application/bloc/auth_state.dart';

/// Email + password login. Dispatches [AuthLoginRequested], navigates on
/// [AuthAuthenticated] via the router redirect.
class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _email = TextEditingController();
  final _password = TextEditingController();

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Log in')),
      body: BlocConsumer<AuthBloc, AuthState>(
        listener: (context, state) {
          if (state is AuthFailure) {
            ScaffoldMessenger.of(context)
              ..hideCurrentSnackBar()
              ..showSnackBar(SnackBar(content: Text(state.message)));
          }
        },
        builder: (context, state) {
          final loading = state is AuthLoading;
          final failure =
              state is AuthFailure ? (state as AuthFailure).message : null;
          return Padding(
            padding: const EdgeInsets.all(16),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  TextFormField(
                    key: const Key('emailField'),
                    controller: _email,
                    decoration: const InputDecoration(labelText: 'Email'),
                    keyboardType: TextInputType.emailAddress,
                    validator: (v) =>
                        (v == null || !v.contains('@')) ? 'Enter an email' : null,
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    key: const Key('passwordField'),
                    controller: _password,
                    decoration: const InputDecoration(labelText: 'Password'),
                    obscureText: true,
                    validator: (v) => (v == null || v.length < 8)
                        ? 'Min 8 characters'
                        : null,
                  ),
                  const SizedBox(height: 16),
                  if (failure != null) ...[
                    ErrorView(message: failure),
                    const SizedBox(height: 12),
                  ],
                  PrimaryButton(
                    label: 'Log in',
                    loading: loading,
                    onPressed: () {
                      if (_formKey.currentState?.validate() ?? false) {
                        context.read<AuthBloc>().add(
                              AuthLoginRequested(
                                email: _email.text.trim(),
                                password: _password.text,
                              ),
                            );
                      }
                    },
                  ),
                  TextButton(
                    key: const Key('goToRegister'),
                    onPressed: () => context.go('/register'),
                    child: const Text('No account? Register'),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
