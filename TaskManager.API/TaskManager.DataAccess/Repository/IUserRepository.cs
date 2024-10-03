using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Models.User;

namespace TaskManager.DataAccess.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(string userId);

        Task<string?> LoginAsync(UserLoginDto loginDto);

        Task<User> RegisterAsync(UserRegistrationDto registrationDto);
    }
}
