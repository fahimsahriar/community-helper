import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../../../shared/widgets/error_view.dart';
import '../../../../shared/widgets/primary_button.dart';
import '../../../auth/application/bloc/auth_bloc.dart';
import '../../../auth/application/bloc/auth_event.dart';
import '../../../auth/application/bloc/auth_state.dart';
import '../../application/cubit/profile_cubit.dart';
import '../../application/cubit/profile_state.dart';

/// Volunteer profile view + edit. New users without a profile are sent to
/// `/onboarding` once load returns null.
class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  bool _redirected = false;

  @override
  void initState() {
    super.initState();
    context.read<ProfileCubit>().load(isOrgAdmin: false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('My profile'),
        actions: [
          IconButton(
            key: const Key('logoutButton'),
            icon: const Icon(Icons.logout),
            onPressed: () =>
                context.read<AuthBloc>().add(const AuthLogoutRequested()),
          ),
        ],
      ),
      body: BlocConsumer<ProfileCubit, ProfileState>(
        listener: (context, state) {
          if (state is ProfileLoaded &&
              state.volunteer == null &&
              !_redirected) {
            _redirected = true;
            context.go('/onboarding');
          }
        },
        builder: (context, state) {
          return switch (state) {
            ProfileInitial() || ProfileLoading() => const Center(
                child: CircularProgressIndicator(),
              ),
            ProfileSaving() => const Center(
                child: CircularProgressIndicator(),
              ),
            ProfileFailure(:final message) => Padding(
                padding: const EdgeInsets.all(16),
                child: ErrorView(
                  message: message,
                  onRetry: () => context
                      .read<ProfileCubit>()
                      .load(isOrgAdmin: false),
                ),
              ),
            ProfileLoaded(:final volunteer) => volunteer == null
                ? const Center(child: CircularProgressIndicator())
                : _ProfileForm(
                    key: ValueKey(volunteer.userId),
                    initialSkills:
                        volunteer.skills.join(', '),
                    initialCauses: volunteer.causes.join(', '),
                    initialAvailability: volunteer.availability,
                    initialLocation: volunteer.location,
                    initialBio: volunteer.bio,
                  ),
          };
        },
      ),
    );
  }
}

class _ProfileForm extends StatefulWidget {
  const _ProfileForm({
    super.key,
    required this.initialSkills,
    required this.initialCauses,
    required this.initialAvailability,
    required this.initialLocation,
    required this.initialBio,
  });

  final String initialSkills;
  final String initialCauses;
  final String initialAvailability;
  final String initialLocation;
  final String initialBio;

  @override
  State<_ProfileForm> createState() => _ProfileFormState();
}

class _ProfileFormState extends State<_ProfileForm> {
  late final TextEditingController _skills;
  late final TextEditingController _causes;
  late final TextEditingController _availability;
  late final TextEditingController _location;
  late final TextEditingController _bio;
  final _formKey = GlobalKey<FormState>();

  @override
  void initState() {
    super.initState();
    _skills = TextEditingController(text: widget.initialSkills);
    _causes = TextEditingController(text: widget.initialCauses);
    _availability = TextEditingController(text: widget.initialAvailability);
    _location = TextEditingController(text: widget.initialLocation);
    _bio = TextEditingController(text: widget.initialBio);
  }

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
    final email = switch (context.watch<AuthBloc>().state) {
      AuthAuthenticated(:final user) => user.email,
      _ => '',
    };
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text('Signed in as $email'),
            const SizedBox(height: 12),
            TextFormField(
              controller: _skills,
              decoration:
                  const InputDecoration(labelText: 'Skills (comma separated)'),
              validator: (v) =>
                  (v == null || v.trim().isEmpty) ? 'Required' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _causes,
              decoration:
                  const InputDecoration(labelText: 'Causes (comma separated)'),
              validator: (v) =>
                  (v == null || v.trim().isEmpty) ? 'Required' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _availability,
              decoration: const InputDecoration(labelText: 'Availability'),
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
            PrimaryButton(
              label: 'Save changes',
              onPressed: () {
                if (_formKey.currentState?.validate() ?? false) {
                  context.read<ProfileCubit>().saveVolunteer(
                        skills: _split(_skills.text),
                        availability: _availability.text.trim(),
                        causes: _split(_causes.text),
                        location: _location.text.trim(),
                        bio: _bio.text.trim(),
                        isUpdate: true,
                      );
                }
              },
            ),
          ],
        ),
      ),
    );
  }
}
