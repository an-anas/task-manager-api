﻿using TaskManager.Models.User;

namespace TaskManager.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterAsync(UserRegistrationDto registrationDto);
        Task<string?> LoginAsync(UserLoginDto loginDto);
    }
}