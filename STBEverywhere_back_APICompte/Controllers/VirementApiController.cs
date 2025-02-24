using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using STBEverywhere_Back_SharedModels.Models;
using STBEverywhere_Back_SharedModels.Models.DTO;
using Microsoft.AspNetCore.Http;
using STBEverywhere_Back_SharedModels;
using STBEverywhere_back_APICompte.Repository.IRepository;

namespace STBEverywhere_back_APICompte.Controllers
{
    [Route("api/VirementApi")]
    [ApiController]
    public class VirementApiController : ControllerBase
    {

        private readonly ICompteRepository _dbCompte;
        private readonly IVirementRepository _dbVirement;

        private readonly ILogger<CompteAPIController> _logger;
        private readonly IMapper _mapper;
        public VirementApiController(ICompteRepository dbCompte, IVirementRepository dbVirement, ILogger<CompteAPIController> logger, IMapper mapper)
        {
            _dbCompte = dbCompte;
            _dbVirement = dbVirement;
            _logger = logger;
            _mapper = mapper;
        }


        [HttpPost("Virement")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> Virement([FromBody] VirementDto virementDto)
        {
            if (virementDto == null || string.IsNullOrEmpty(virementDto.RIB_Emetteur) ||
                string.IsNullOrEmpty(virementDto.RIB_Recepteur) || virementDto.Montant <= 0)
            {
                return BadRequest(new { message = "RIB émetteur, RIB récepteur et montant sont obligatoires et le montant doit être positif." });
            }
            var emetteur = (await _dbCompte.GetAllAsync(c => c.RIB == virementDto.RIB_Emetteur)).FirstOrDefault();
            var recepteur = (await _dbCompte.GetAllAsync(c => c.RIB == virementDto.RIB_Recepteur)).FirstOrDefault();

            //var emetteur = await _context.Compte.FirstOrDefaultAsync(c => c.RIB == virementDto.RIB_Emetteur);
            //var recepteur = await _context.Compte.FirstOrDefaultAsync(c => c.RIB == virementDto.RIB_Recepteur);

            if (emetteur == null || recepteur == null)
            {
                return NotFound(new { message = "Compte émetteur ou récepteur introuvable." });
            }

            if (emetteur.Solde < virementDto.Montant)
            {
                return BadRequest(new { message = "Solde insuffisant sur le compte émetteur." });
            }
            await _dbVirement.BeginTransactionAsync();
            //using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    emetteur.Solde -= virementDto.Montant;
                    recepteur.Solde += virementDto.Montant;


                    await _dbCompte.UpdateAsync(emetteur);
                    await _dbCompte.UpdateAsync(recepteur);
                    /*_context.Compte.Update(emetteur);
                    _context.Compte.Update(recepteur);*/

                    var virement = new Virement
                    {
                        RIB_Emetteur = virementDto.RIB_Emetteur,
                        RIB_Recepteur = virementDto.RIB_Recepteur,
                        Montant = virementDto.Montant,
                        DateVirement = DateTime.Now,
                        Statut = "Réussi",
                        Description = virementDto.Description
                    };

                    await _dbVirement.CreateAsync(virement);//ajout /creation d'un virement
                    await _dbVirement.CommitTransactionAsync();
                    /*_context.Virement.Add(virement);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();*/

                    return Ok(new { message = "Virement effectué avec succès." });
                }
                catch (Exception)
                {
                    await _dbVirement.RollbackTransactionAsync();
                    return StatusCode(500, new { message = "Une erreur est survenue lors du virement." });
                }
            }
        }

    }
}
