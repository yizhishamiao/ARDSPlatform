using ARDSPlatform.DAL;
using ARDSPlatform.Models;
using System.Linq;

namespace ARDSPlatform.Services
{
    public class UserService
    {
        private readonly ards_DbContext _context;

        public UserService(ards_DbContext context)
        {
            _context = context;
        }

        public Users GetUserById(int userId)
        {
            return _context.tb_users.Find(userId);
        }

        public Users GetUserByAccount(string account)
        {
            return _context.tb_users.FirstOrDefault(u => u.Account == account);
        }

        // 其他获取用户信息的方法
    }
}