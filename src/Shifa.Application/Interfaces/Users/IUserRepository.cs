using Shifa.Application.DTOs.Users;

namespace Shifa.Application.Interfaces.Users;

public interface IUserRepository
{
    Task<UserProfileResponse?> GetProfileAsync(int userId);

    Task<bool> UpdateProfileAsync(
        int userId,
        UpdateUserProfileRequest request);
    Task ChangePasswordAsync(
    int userId,
    ChangePasswordRequest request);

}