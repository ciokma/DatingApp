using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

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
    
        public async Task<IEnumerable<User>> GetUsers()
        {
            var user = await _context.Users.Include(p=> p.Photos).ToListAsync();
            //var user = await _context.Users.ToListAsync();
            return user;
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
     
    }
}