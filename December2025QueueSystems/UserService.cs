using System;

namespace December2025QueueSystems
{
    public class UserService(IUserRepo userRepo)
    {
        public void Register(User user)
        {
            userRepo.Create(user);
        }
    }
}