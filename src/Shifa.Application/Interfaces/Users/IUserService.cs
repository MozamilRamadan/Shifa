using Shifa.Application.DTOs.Users;

namespace Shifa.Application.Interfaces.Users;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(int userId);

    Task UpdateProfileAsync(
        int userId,
        UpdateUserProfileRequest request);
    Task ChangePasswordAsync(
    int userId,
    ChangePasswordRequest request);

}