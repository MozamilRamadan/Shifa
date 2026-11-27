using Shifa.Application.DTOs.Users;
using Shifa.Application.Exceptions;
using Shifa.Application.Interfaces.Users;

namespace Shifa.Application.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileResponse> GetProfileAsync(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        var profile = await _userRepository.GetProfileAsync(userId);

        if (profile is null)
            throw new NotFoundException("User not found.");

        return profile;
    }

    public async Task UpdateProfileAsync(
        int userId,
        UpdateUserProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException(
                "First name and last name are required.");
        }

        request.FirstName = request.FirstName.Trim();
        request.LastName = request.LastName.Trim();

        var updated = await _userRepository.UpdateProfileAsync(
            userId, request);

        if (!updated)
            throw new NotFoundException("User not found.");
    }

    public async Task ChangePasswordAsync(
    int userId,
    ChangePasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            throw new ArgumentException("Current password is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new ArgumentException("New password is required.");

        if (request.NewPassword != request.ConfirmNewPassword)
            throw new ArgumentException("Password confirmation does not match.");

        if (request.CurrentPassword == request.NewPassword)
            throw new ArgumentException(
                "New password must be different from current password.");

        await _userRepository.ChangePasswordAsync(userId, request);
    }

}