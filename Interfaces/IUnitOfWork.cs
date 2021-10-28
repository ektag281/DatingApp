using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Api.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository {get; }
        IMessageRepository MessageRepository {get; }
        ILikesRepository LikesRepository {get; }
        Task<bool> Complete();

        //For trachikng any change
        bool HasChanges();
    }
}