using System.Collections.Generic;
using System.Threading.Tasks;
using Kartverket.Database.Models;
using Kartverket.Models;

namespace Kartverket.Services;
public interface IUserService
{
    Task<int> GetUserIdAsync(int id);
    Task<Users> GetUserAsync(int id);
    Task<Users> GetUserByUsernameAsync(string username);
    

}