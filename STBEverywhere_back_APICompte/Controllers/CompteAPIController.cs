using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using STBEverywhere_Back_SharedModels;

using STBEverywhere_Back_SharedModels.Models.DTO;
using STBEverywhere_back_APICompte.Repository.IRepository;
using System.Numerics;

namespace STBEverywhere_back_APICompte.Controllers
{
    [Route("api/CompteApi")]
    [ApiController]
    public class CompteAPIController : ControllerBase
    {


        private readonly ICompteRepository _dbCompte;
        private readonly IVirementRepository _dbVirement;

        private readonly ILogger<CompteAPIController> _logger;
        private readonly IMapper _mapper;
        public CompteAPIController(ICompteRepository dbCompte, IVirementRepository dbVirement, ILogger<CompteAPIController> logger, IMapper mapper)
        {
            _dbCompte = dbCompte;
            _dbVirement = dbVirement;
            _logger = logger;
            _mapper = mapper;
        }



        [HttpGet("{numCin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComptesByCin(string numCin)
        {
            //var comptes = await _context.Compte
            var comptes = await _dbCompte.GetAllAsync(c => c.NumCin == numCin && c.Statut != "Clôturé");
            /*.Where(c => c.NumCin == numCin && c.statut != "Clôturé")
            .Select(c => new
            {
                c.RIB,
                c.type,
                c.solde
            })
            .ToListAsync();*/

            if (comptes == null || !comptes.Any())
            {
                return NotFound(new { message = "Aucun compte actif trouvé pour vous." });
            }
            _logger.LogInformation("Getting all comptes");
            return Ok(comptes);
        }


        [HttpPost("CreateCompte")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCompte([FromBody] CreateCompteDto compteDto)
        {
            if (compteDto == null || string.IsNullOrEmpty(compteDto.NumCin) || string.IsNullOrEmpty(compteDto.type))
            {
                _logger.LogError("NumCin et Type sont obligatoires");
                return BadRequest(new { message = "NumCin et Type sont obligatoires." });
            }

            if (compteDto.type.ToLower() == "epargne")
            {
                var epargneCount = (await _dbCompte.GetAllAsync(c => c.NumCin == compteDto.NumCin && c.Type.ToLower() == "epargne")).Count;
                /*var epargneCount = await _context.Compte
                    .Where(c => c.NumCin == compteDto.NumCin && c.type.ToLower() == "epargne")
                    .CountAsync();*/

                if (epargneCount >= 3)
                {
                    return BadRequest(new { message = "Vous ne pouvez pas avoir plus de 3 comptes d'épargne." });
                }
            }

            string generatedRIB = GenerateUniqueRIB();
            decimal initialSolde = compteDto.type.ToLower() == "epargne" ? 10 : 0;
            // Utilisation d'AutoMapper pour convertir compteDto en Compte
            var compte = _mapper.Map<Compte>(compteDto);

            // Ajout des valeurs manquantes
            compte.RIB = generatedRIB;
            compte.Solde = initialSolde;
            compte.DateCreation = DateTime.Now;
            compte.Statut = "Actif";
            /*var compte = new Compte
            {
                RIB = generatedRIB,
                NumCin = compteDto.NumCin,
                type = compteDto.type,
                solde = initialSolde,
                date_creation = DateTime.Now,
                statut = "Actif"
            };*/
            await _dbCompte.CreateAsync(compte);
            await _dbCompte.SaveAsync();
            //_context.Compte.Add(compte);
            // await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompteByRIB), new { rib = compte.RIB }, compte);
        }

        private string GenerateUniqueRIB()
        {
            string guidString = Guid.NewGuid().ToString("N");
            string rib = string.Concat(guidString.Where(c => char.IsDigit(c))).Substring(0, 20);
            return rib;
        }

        [HttpGet("GetByRIB/{rib}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCompteByRIB(string rib)
        {
            var compte = await _dbCompte.GetAllAsync(c => c.RIB == rib);
            /*var compte = await _context.Compte
                .Where(c => c.RIB == rib)
                .Select(c => new
                {
                    c.RIB,
                    c.type,
                    c.solde,
                    c.date_creation,
                    c.statut
                })
                .ToListAsync();*/


            if (compte == null || !compte.Any())
            {
                return NotFound(new { message = "Compte introuvable." });
            }

            return Ok(compte);
        }


        [HttpPut("Cloturer/{rib}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CloturerCompte(string rib)
        {
            var compte = ((await _dbCompte.GetAllAsync(c => c.RIB == rib)).FirstOrDefault());
            //var compte = await _context.Compte.FirstOrDefaultAsync(c => c.RIB == rib);

            if (compte == null)
            {
                return NotFound(new { message = "Compte introuvable." });
            }

            if (compte.Solde != 0)
            {
                ModelState.AddModelError("", "Vous devez mettre votre compte à zéro puis réessayer de le clôturer.");
                return BadRequest(ModelState);
            }

            compte.Statut = "Clôturé";
            //await _context.SaveChangesAsync();
            await _dbCompte.SaveAsync();
            return Ok(new { message = "Le compte a été clôturé avec succès." });
        }





    }





}
