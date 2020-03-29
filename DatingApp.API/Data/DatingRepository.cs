using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository()
        {
        }

        //Crear constructor para acceder al contexto de las tablas  
        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId== userId && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhoto(int userId)
        {
         return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p=>p.isMain);
        }

        public async Task<Photo> GetMainPhotoFromUser(int userId)
        {
          return await _context.Photos.Where(p => p.Id == userId).FirstOrDefaultAsync(p=>p.isMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p=>p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
           var user = await _context.Users.Include(p=>p.Photos).FirstOrDefaultAsync(u=>u.Id ==id);
           /* var user = await _context.Users.FirstOrDefaultAsync(u=>u.Id ==id);
            var photo = await _context.Photos.FirstOrDefaultAsync(u => u.UserId == user.Id);
            List<Photo> photos = new List<Photo>();
            photos.Add(photo);
            user.Photos= photos;
            */
            return user; 
        }
    
        public async Task<PageListed<User>> GetUsers(UserParams userParams)
        {
            /*
            var user = await _context.Users.Include(p=> p.Photos).ToListAsync();
            //var user = await _context.Users.ToListAsync();
            return user;*/

            //nuevos cambios fase 14 , paginando
            var users = _context.Users.Include(p=>p.Photos)
            .OrderByDescending(u=>u.LastActivity)
            .AsQueryable();
            //omitir al usuario actual de la consulta
            users = users.Where(u=>u.Id!=userParams.UserId);
            //filtrando los usuarios por el genero
            users = users.Where(u=>u.Gender == userParams.Gender);

            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u=> userLikers.Contains(u.Id));
            }
            if(userParams.Likees)
            {
              var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
              users = users.Where(u=> userLikees.Contains(u.Id));
            }

            //adicionales filtros
            if(userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge-1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }
            if(!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                          users = users.OrderByDescending(u => u.Created);
                    break;
                    default:
                          users = users.OrderByDescending(u => u.LastActivity);
                    break;
                }
            }
            return await PageListed<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }
        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users.Include(x => x.Likers).Include(x => x.Likees)
            .FirstOrDefaultAsync(u=>u.Id == id);
            if(likers)
            {
                return user.Likers.Where(u=>u.LikeeId==id).Select(i=>i.LikerId);
            }
            else {
                return user.Likees.Where(u=>u.LikerId == id).Select(i=>i.LikeeId);
            }
        }
        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<PageListed<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId 
                        && !u.RecipientDeleted);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId 
                        && !u.SenderDeleted);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId 
                        && !u.RecipientDeleted && !u.IsRead);
                    break;
            }

            messages = messages.OrderByDescending(d => d.MessageSent);

            return await PageListed<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                .Include(u =>u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where( m => m.RecipientId == userId && !m.RecipientDeleted && m.SenderId == recipientId
                    || m.RecipientId == recipientId && m.SenderId == userId && !m.SenderDeleted)
                .OrderBy(u => u.MessageSent)
                .ToListAsync();
            return messages;
        }
    }
}