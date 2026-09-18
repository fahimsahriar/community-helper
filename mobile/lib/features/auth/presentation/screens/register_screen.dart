import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../../../shared/widgets/error_view.dart';
import '../../../../shared/widgets/primary_button.dart';
import '../../application/bloc/auth_bloc.dart';
import '../../application/bloc/auth_event.dart';
import '../../application/bloc/auth_state.dart';
import '../../data/models/user_model.dart';

/// Register with role picker (Volunteer / Organization admin).
class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _email = TextEditingController();
  final _password = TextEditingController();
  String _role = AppRoles.volunteer;

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Register')),
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
                  const SizedBox(height: 12),
                  SegmentedButton<String>(
                    key: const Key('rolePicker'),
                    segments: const [
                      ButtonSegment(
                        value: AppRoles.volunteer,
                        label: Text('Volunteer'),
                      ),
                      ButtonSegment(
                        value: AppRoles.orgAdmin,
                        label: Text('Organization'),
                      ),
                    ],
                    selected: {_role},
                    onSelectionChanged: (s) => setState(() {
                      _role = s.first;
                    }),
                  ),
                  const SizedBox(height: 16),
                  if (failure != null) ...[
                    ErrorView(message: failure),
                    const SizedBox(height: 12),
                  ],
                  PrimaryButton(
                    label: 'Create account',
                    loading: loading,
                    onPressed: () {
                      if (_formKey.currentState?.validate() ?? false) {
                        context.read<AuthBloc>().add(
                              AuthRegisterRequested(
                                email: _email.text.trim(),
                                password: _password.text,
                                role: _role,
                              ),
                            );
                      }
                    },
                  ),
                  TextButton(
                    onPressed: () => context.go('/login'),
                    child: const Text('Have an account? Log in'),
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
