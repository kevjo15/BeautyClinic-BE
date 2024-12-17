using System;

namespace Application_Layer.Queries.UserQueries.GetUserById
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string userId)
            : base($"User with ID {userId} not found.")
        {
        }
    }
} 