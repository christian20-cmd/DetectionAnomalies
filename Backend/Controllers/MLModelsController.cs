using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MLModelsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MLModelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/mlmodels
        // Récupérer tous les modèles ML
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MLModel>>> GetAllModels()
        {
            var models = await _context.MLModels
                .OrderByDescending(m => m.TrainingDate)
                .ToListAsync();

            return Ok(models);
        }

        // GET /api/mlmodels/5
        // Récupérer un modèle par son Id
        [HttpGet("{id}")]
        public async Task<ActionResult<MLModel>> GetModel(int id)
        {
            var model = await _context.MLModels.FindAsync(id);

            if (model == null)
                return NotFound($"Modèle ML avec l'Id {id} introuvable.");

            return Ok(model);
        }

        // GET /api/mlmodels/active
        // Récupérer le modèle actuellement actif
        [HttpGet("active")]
        public async Task<ActionResult<MLModel>> GetActiveModel()
        {
            var model = await _context.MLModels
                .FirstOrDefaultAsync(m => m.IsActive == true);

            if (model == null)
                return NotFound("Aucun modèle actif trouvé.");

            return Ok(model);
        }

        // POST /api/mlmodels
        // Enregistrer un nouveau modèle ML après entraînement
        [HttpPost]
        public async Task<ActionResult<MLModel>> CreateModel(MLModel model)
        {
            // Vérifier que la version n'existe pas déjà
            var exists = await _context.MLModels
                .AnyAsync(m => m.Version == model.Version);

            if (exists)
                return BadRequest($"Un modèle avec la version {model.Version} existe déjà.");

            model.TrainingDate = DateTime.UtcNow;
            model.TotalAnomaliesDetected = 0;
            model.IsActive = false;

            _context.MLModels.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetModel), new { id = model.Id }, model);
        }

        // PUT /api/mlmodels/5/activate
        // Activer un modèle et désactiver tous les autres
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateModel(int id)
        {
            var model = await _context.MLModels.FindAsync(id);

            if (model == null)
                return NotFound($"Modèle ML avec l'Id {id} introuvable.");

            // Désactiver tous les modèles existants
            var allModels = await _context.MLModels.ToListAsync();
            foreach (var m in allModels)
            {
                m.IsActive = false;
            }

            // Activer uniquement le modèle sélectionné
            model.IsActive = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT /api/mlmodels/5
        // Mettre à jour les métriques d'un modèle
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModel(int id, MLModel updatedModel)
        {
            var model = await _context.MLModels.FindAsync(id);

            if (model == null)
                return NotFound($"Modèle ML avec l'Id {id} introuvable.");

            model.Accuracy = updatedModel.Accuracy;
            model.Precision = updatedModel.Precision;
            model.Recall = updatedModel.Recall;
            model.F1Score = updatedModel.F1Score;
            model.Notes = updatedModel.Notes;
            model.ModelFilePath = updatedModel.ModelFilePath;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/mlmodels/5
        // Supprimer un modèle ML
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            var model = await _context.MLModels.FindAsync(id);

            if (model == null)
                return NotFound($"Modèle ML avec l'Id {id} introuvable.");

            // On ne peut pas supprimer le modèle actif
            if (model.IsActive)
                return BadRequest("Impossible de supprimer le modèle actuellement actif.");

            _context.MLModels.Remove(model);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/mlmodels/compare
        // Comparer les performances de tous les modèles
        [HttpGet("compare")]
        public async Task<ActionResult> CompareModels()
        {
            var models = await _context.MLModels
                .OrderByDescending(m => m.F1Score)
                .Select(m => new
                {
                    m.Id,
                    m.Version,
                    m.Algorithm,
                    m.Accuracy,
                    m.Precision,
                    m.Recall,
                    m.F1Score,
                    m.IsActive,
                    m.TotalAnomaliesDetected,
                    m.TrainingDate
                })
                .ToListAsync();

            return Ok(models);
        }
    }
}