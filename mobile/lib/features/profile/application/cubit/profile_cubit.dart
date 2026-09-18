import 'package:dio/dio.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../../core/error/api_exception.dart';
import '../../domain/repositories/profile_repository.dart';
import 'profile_state.dart';

/// Loads and saves the current user's Volunteer / Organization profile.
class ProfileCubit extends Cubit<ProfileState> {
  ProfileCubit(this._repository) : super(const ProfileInitial());

  final ProfileRepository _repository;

  Future<void> load({required bool isOrgAdmin}) async {
    emit(const ProfileLoading());
    try {
      if (isOrgAdmin) {
        final org = await _repository.getMyOrganization();
        if (isClosed) {
          return;
        }
        emit(ProfileLoaded(organization: org));
      } else {
        final volunteer = await _repository.getMyVolunteerProfile();
        if (isClosed) {
          return;
        }
        emit(ProfileLoaded(volunteer: volunteer));
      }
    } on DioException catch (e) {
      if (isClosed) {
        return;
      }
      emit(ProfileFailure(mapDioErrorToFailure(e).message));
    }
  }

  Future<void> saveVolunteer({
    required List<String> skills,
    required String availability,
    required List<String> causes,
    required String location,
    required String bio,
    required bool isUpdate,
  }) async {
    emit(const ProfileSaving());
    try {
      final saved = await _repository.saveVolunteerProfile(
        skills: skills,
        availability: availability,
        causes: causes,
        location: location,
        bio: bio,
        isUpdate: isUpdate,
      );
      if (isClosed) {
        return;
      }
      emit(ProfileLoaded(volunteer: saved));
    } on DioException catch (e) {
      if (isClosed) {
        return;
      }
      emit(ProfileFailure(mapDioErrorToFailure(e).message));
    }
  }

  Future<void> saveOrganization({
    String? id,
    required String name,
    required String type,
    required List<String> causeTags,
    required String location,
    required String description,
  }) async {
    emit(const ProfileSaving());
    try {
      final saved = await _repository.saveOrganization(
        id: id,
        name: name,
        type: type,
        causeTags: causeTags,
        location: location,
        description: description,
      );
      if (isClosed) {
        return;
      }
      emit(ProfileLoaded(organization: saved));
    } on DioException catch (e) {
      if (isClosed) {
        return;
      }
      emit(ProfileFailure(mapDioErrorToFailure(e).message));
    }
  }
}
