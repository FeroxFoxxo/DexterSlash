using DexterSlash.Databases.Models;

namespace DexterSlash.Databases.Repositories
{
    public class ModMailRepository : BaseRepository<ModMailRepository>
    {
        public ModMailRepository(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public async Task<ModMail> CreateModMail(string message, ulong userID)
        {
            var mail = _context.ModMail.Add(
                new()
                {
                    Message = message,
                    UserID = userID
                }
            );

            await _context.SaveChangesAsync();

            return mail.Entity;
        }

        public async Task UpdateModMail(int trackerID, ulong messageID)
        {
            var modMail = await _context.ModMail.FindAsync(trackerID);

            modMail.MessageID = messageID;

            await _context.SaveChangesAsync();
        }

    }
}