import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../../../shared/widgets/error_view.dart';
import '../../../../shared/widgets/primary_button.dart';
import '../../application/cubit/profile_cubit.dart';
import '../../application/cubit/profile_state.dart';

/// Volunteer onboarding: skills, causes, availability, location, bio.
/// On save → `/profile`.
class OnboardingScreen extends StatefulWidget {
  const OnboardingScreen({super.key});

  @override
  State<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends State<OnboardingScreen> {
  final _formKey = GlobalKey<FormState>();
  final _skills = TextEditingController();
  final _causes = TextEditingController();
  final _availability = TextEditingController();
  final _location = TextEditingController();
  final _bio = TextEditingController();

  @override
  void dispose() {
    _skills.dispose();
    _causes.dispose();
    _availability.dispose();
    _location.dispose();
    _bio.dispose();
    super.dispose();
  }

  List<String> _split(String raw) => raw
      .split(',')
      .map((s) => s.trim())
      .where((s) => s.isNotEmpty)
      .toList();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Complete your profile')),
      body: BlocConsumer<ProfileCubit, ProfileState>(
        listener: (context, state) {
          if (state is ProfileLoaded) {
            context.go('/profile');
          }
          if (state is ProfileFailure) {
            ScaffoldMessenger.of(context)
              ..hideCurrentSnackBar()
              ..showSnackBar(SnackBar(content: Text(state.message)));
          }
        },
        builder: (context, state) {
          final saving = state is ProfileSaving;
          final failure =
              state is ProfileFailure ? (state as ProfileFailure).message : null;
          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  TextFormField(
                    controller: _skills,
                    decoration: const InputDecoration(
                      labelText: 'Skills (comma separated)',
                    ),
                    validator: (v) =>
                        (v == null || v.trim().isEmpty) ? 'Required' : null,
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _causes,
                    decoration: const InputDecoration(
                      labelText: 'Causes (comma separated)',
                    ),
                    validator: (v) =>
                        (v == null || v.trim().isEmpty) ? 'Required' : null,
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _availability,
                    decoration: const InputDecoration(
                      labelText: 'Availability',
                    ),
                    validator: (v) =>
                        (v == null || v.trim().isEmpty) ? 'Required' : null,
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _location,
                    decoration: const InputDecoration(labelText: 'Location'),
                    validator: (v) =>
                        (v == null || v.trim().isEmpty) ? 'Required' : null,
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _bio,
                    decoration: const InputDecoration(labelText: 'Bio'),
                    maxLines: 3,
                  ),
                  const SizedBox(height: 16),
                  if (failure != null) ...[
                    ErrorView(message: failure),
                    const SizedBox(height: 12),
                  ],
                  PrimaryButton(
                    label: 'Save profile',
                    loading: saving,
                    onPressed: () {
                      if (_formKey.currentState?.validate() ?? false) {
                        context.read<ProfileCubit>().saveVolunteer(
                              skills: _split(_skills.text),
                              availability: _availability.text.trim(),
                              causes: _split(_causes.text),
                              location: _location.text.trim(),
                              bio: _bio.text.trim(),
                              isUpdate: false,
                            );
                      }
                    },
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
