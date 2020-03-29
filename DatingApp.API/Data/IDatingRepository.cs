using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Helpers;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
         void Add<T>(T entity) where T: class;
         void Delete<T>(T entity) where T: class;
         Task<bool> SaveAll();
         Task<PageListed<User>> GetUsers(UserParams userParams);
         Task<User> GetUser(int id);
         Task<Photo> GetPhoto(int id);
         Task<Photo> GetMainPhoto(int userId);
         Task<Photo> GetMainPhotoFromUser(int userId);
         Task<Like> GetLike(int userId, int recipientId);
         Task<Message> GetMessage(int id);
         Task<PageListed<Message>> GetMessagesForUser(MessageParams messageParams);
         Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId);
    }
}