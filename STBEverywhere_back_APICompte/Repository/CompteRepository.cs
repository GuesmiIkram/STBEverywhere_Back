using Microsoft.EntityFrameworkCore;
using STBEverywhere_Back_SharedModels.Data;

using STBEverywhere_back_APICompte.Repository.IRepository;
using System.Linq;
using System.Linq.Expressions;
using STBEverywhere_Back_SharedModels;
namespace STBEverywhere_back_APICompte.Repository
{
    public class CompteRepository : Repository<Compte>, ICompteRepository
    {
        private readonly ApplicationDbContext _db;

        public CompteRepository(ApplicationDbContext db) : base(db)
        { _db = db; }
        /*public async Task CreateAsync(Compte entity)
        {
            await _db.Comptes.AddAsync(entity);
            await SaveAsync();
        }*/
        public async Task<Compte> GetByRibAsync(string rib)
        {
            return await _db.Comptes.FirstOrDefaultAsync(c => c.RIB == rib);
        }

        public async Task<Compte> UpdateAsync(Compte entity)
        {
            _db.Comptes.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
        public async Task<List<Compte>> GetAllAsync(Expression<Func<Compte, bool>> filter = null)
        {
            IQueryable<Compte> query = _db.Comptes;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();

        }

        /* public async Task SaveAsync()
         {
             await _db.SaveChangesAsync();
         }*/



    }



}