import 'package:dio/dio.dart';

import '../../domain/repositories/profile_repository.dart';
import '../datasources/profile_remote_data_source.dart';
import '../models/organization_model.dart';
import '../models/volunteer_profile_model.dart';

/// Returns null on 404 for "my" getters (profile not created yet).
class ProfileRepositoryImpl implements ProfileRepository {
  ProfileRepositoryImpl(this._remote);

  final ProfileRemoteDataSource _remote;

  @override
  Future<VolunteerProfileModel?> getMyVolunteerProfile() async {
    try {
      return await _remote.getMyVolunteerProfile();
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        return null;
      }
      rethrow;
    }
  }

  @override
  Future<VolunteerProfileModel> saveVolunteerProfile({
    required List<String> skills,
    required String availability,
    required List<String> causes,
    required String location,
    required String bio,
    required bool isUpdate,
  }) {
    if (isUpdate) {
      return _remote.updateMyVolunteerProfile(
        skills: skills,
        availability: availability,
        causes: causes,
        location: location,
        bio: bio,
      );
    }
    return _remote.createMyVolunteerProfile(
      skills: skills,
      availability: availability,
      causes: causes,
      location: location,
      bio: bio,
    );
  }

  @override
  Future<OrganizationModel?> getMyOrganization() async {
    try {
      return await _remote.getMyOrganization();
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        return null;
      }
      rethrow;
    }
  }

  @override
  Future<OrganizationModel> saveOrganization({
    String? id,
    required String name,
    required String type,
    required List<String> causeTags,
    required String location,
    required String description,
  }) {
    if (id == null) {
      return _remote.registerOrganization(
        name: name,
        type: type,
        causeTags: causeTags,
        location: location,
        description: description,
      );
    }
    return _remote.updateOrganization(
      id: id,
      name: name,
      type: type,
      causeTags: causeTags,
      location: location,
      description: description,
    );
  }
}
