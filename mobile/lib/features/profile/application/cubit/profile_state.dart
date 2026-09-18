import 'package:equatable/equatable.dart';

import '../../data/models/organization_model.dart';
import '../../data/models/volunteer_profile_model.dart';

/// Simple Cubit state for profile view/edit (local UI + async save).
sealed class ProfileState extends Equatable {
  const ProfileState();

  @override
  List<Object?> get props => [];
}

final class ProfileInitial extends ProfileState {
  const ProfileInitial();
}

final class ProfileLoading extends ProfileState {
  const ProfileLoading();
}

final class ProfileLoaded extends ProfileState {
  const ProfileLoaded({this.volunteer, this.organization});

  final VolunteerProfileModel? volunteer;
  final OrganizationModel? organization;

  @override
  List<Object?> get props => [volunteer, organization];
}

final class ProfileSaving extends ProfileState {
  const ProfileSaving();
}

final class ProfileFailure extends ProfileState {
  const ProfileFailure(this.message);

  final String message;

  @override
  List<Object?> get props => [message];
}
