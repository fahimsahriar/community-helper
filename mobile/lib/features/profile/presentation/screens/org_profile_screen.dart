import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../../shared/widgets/error_view.dart';
import '../../../../shared/widgets/primary_button.dart';
import '../../../auth/application/bloc/auth_bloc.dart';
import '../../../auth/application/bloc/auth_event.dart';
import '../../application/cubit/profile_cubit.dart';
import '../../application/cubit/profile_state.dart';
import '../../data/models/organization_model.dart';

/// Organization admin profile: register on first run, then view/edit.
/// Shows the pending-verification badge while `verifiedAt == null`.
class OrgProfileScreen extends StatefulWidget {
  const OrgProfileScreen({super.key});

  @override
  State<OrgProfileScreen> createState() => _OrgProfileScreenState();
}

class _OrgProfileScreenState extends State<OrgProfileScreen> {
  @override
  void initState() {
    super.initState();
    context.read<ProfileCubit>().load(isOrgAdmin: true);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Organization'),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () =>
                context.read<AuthBloc>().add(const AuthLogoutRequested()),
          ),
        ],
      ),
      body: BlocBuilder<ProfileCubit, ProfileState>(
        builder: (context, state) {
          return switch (state) {
            ProfileInitial() || ProfileLoading() || ProfileSaving() =>
              const Center(child: CircularProgressIndicator()),
            ProfileFailure(:final message) => Padding(
                padding: const EdgeInsets.all(16),
                child: ErrorView(
                  message: message,
                  onRetry: () =>
                      context.read<ProfileCubit>().load(isOrgAdmin: true),
                ),
              ),
            ProfileLoaded(:final organization) => _OrgForm(
                key: ValueKey(organization?.id ?? 'new'),
                initial: organization,
              ),
          };
        },
      ),
    );
  }
}

class _OrgForm extends StatefulWidget {
  const _OrgForm({super.key, this.initial});

  final OrganizationModel? initial;

  @override
  State<_OrgForm> createState() => _OrgFormState();
}

class _OrgFormState extends State<_OrgForm> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _name;
  late final TextEditingController _type;
  late final TextEditingController _causeTags;
  late final TextEditingController _location;
  late final TextEditingController _description;

  @override
  void initState() {
    super.initState();
    final i = widget.initial;
    _name = TextEditingController(text: i?.name ?? '');
    _type = TextEditingController(text: i?.type ?? '');
    _causeTags = TextEditingController(text: i?.causeTags.join(', ') ?? '');
    _location = TextEditingController(text: i?.location ?? '');
    _description = TextEditingController(text: i?.description ?? '');
  }

  @override
  void dispose() {
    _name.dispose();
    _type.dispose();
    _causeTags.dispose();
    _location.dispose();
    _description.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final pending = widget.initial?.isPendingVerification ?? false;
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            if (pending)
              const Card(
                child: ListTile(
                  leading: Icon(Icons.pending_outlined),
                  title: Text('Pending verification'),
                  subtitle: Text(
                    'Your organization is visible but awaiting trust review.',
                  ),
                ),
              ),
            TextFormField(
              controller: _name,
              decoration: const InputDecoration(labelText: 'Organization name'),
              validator: (v) =>
                  (v == null || v.trim().isEmpty) ? 'Required' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _type,
              decoration: const InputDecoration(labelText: 'Type'),
              validator: (v) =>
                  (v == null || v.trim().isEmpty) ? 'Required' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _causeTags,
              decoration: const InputDecoration(
                labelText: 'Cause tags (comma separated)',
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
              controller: _description,
              decoration: const InputDecoration(labelText: 'Description'),
              maxLines: 3,
              validator: (v) =>
                  (v == null || v.trim().isEmpty) ? 'Required' : null,
            ),
            const SizedBox(height: 16),
            PrimaryButton(
              label: widget.initial == null
                  ? 'Register organization'
                  : 'Save changes',
              onPressed: () {
                if (_formKey.currentState?.validate() ?? false) {
                  final tags = _causeTags.text
                      .split(',')
                      .map((s) => s.trim())
                      .where((s) => s.isNotEmpty)
                      .toList();
                  context.read<ProfileCubit>().saveOrganization(
                        id: widget.initial?.id,
                        name: _name.text.trim(),
                        type: _type.text.trim(),
                        causeTags: tags,
                        location: _location.text.trim(),
                        description: _description.text.trim(),
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
