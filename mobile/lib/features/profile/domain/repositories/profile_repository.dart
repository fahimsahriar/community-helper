import '../../data/models/organization_model.dart';
import '../../data/models/volunteer_profile_model.dart';

abstract class ProfileRepository {
  Future<VolunteerProfileModel?> getMyVolunteerProfile();
  Future<VolunteerProfileModel> saveVolunteerProfile({
    required List<String> skills,
    required String availability,
    required List<String> causes,
    required String location,
    required String bio,
    required bool isUpdate,
  });

  Future<OrganizationModel?> getMyOrganization();
  Future<OrganizationModel> saveOrganization({
    String? id,
    required String name,
    required String type,
    required List<String> causeTags,
    required String location,
    required String description,
  });
}
