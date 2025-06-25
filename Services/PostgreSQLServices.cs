using CompetitionApp.Data;
using CompetitionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CompetitionApp.Services
{
    public interface ICompetitionService
    {
        Task<Competition> CreateCompetitionAsync(string name, string description, DateTime competitionDate);
        Task<Competition?> GetCompetitionByIdAsync(int id);
        Task<IEnumerable<Competition>> GetAllCompetitionsAsync();
        Task<IEnumerable<Competition>> GetCompetitionsByDateRangeAsync(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<Competition>> SearchCompetitionsByNameAsync(string name);
        Task<Competition> UpdateCompetitionAsync(Competition competition);
        Task DeleteCompetitionAsync(int id);
    }

    public class CompetitionService : ICompetitionService
    {
        private readonly CompetitionDbContext _context;
        private readonly ILogger<CompetitionService> _logger;

        public CompetitionService(CompetitionDbContext context, ILogger<CompetitionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Competition> CreateCompetitionAsync(string name, string description, DateTime competitionDate)
        {
            try
            {
                _logger.LogInformation("Criando nova competição: {Name}", name);

                var competition = new Competition
                {
                    Name = name,
                    Description = description,
                    CompetitionDate = competitionDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Competitions.Add(competition);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Competição criada com sucesso. ID: {Id}", competition.Id);
                return competition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar competição: {Name}", name);
                throw;
            }
        }

        public async Task<Competition?> GetCompetitionByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando competição por ID: {Id}", id);

                var competition = await _context.Competitions
                    .Include(c => c.Participants)
                        .ThenInclude(cp => cp.Participant)
                    .Include(c => c.Results)
                        .ThenInclude(r => r.Participant)
                    .Include(c => c.FinalResults)
                        .ThenInclude(fr => fr.Participant)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (competition != null)
                {
                    _logger.LogInformation("Competição encontrada: {Name}", competition.Name);
                }
                else
                {
                    _logger.LogWarning("Competição não encontrada para ID: {Id}", id);
                }

                return competition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar competição por ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Competition>> GetAllCompetitionsAsync()
        {
            try
            {
                _logger.LogInformation("Buscando todas as competições");

                var competitions = await _context.Competitions
                    .Include(c => c.Participants)
                        .ThenInclude(cp => cp.Participant)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Encontradas {Count} competições", competitions.Count);
                return competitions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todas as competições");
                throw;
            }
        }

        public async Task<IEnumerable<Competition>> GetCompetitionsByDateRangeAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                _logger.LogInformation("Buscando competições por intervalo de datas: {StartDate} - {EndDate}", startDate, endDate);

                var query = _context.Competitions.AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= endDate.Value.AddDays(1));
                }

                var competitions = await query
                    .Include(c => c.Participants)
                        .ThenInclude(cp => cp.Participant)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Encontradas {Count} competições no intervalo de datas", competitions.Count);
                return competitions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar competições por intervalo de datas");
                throw;
            }
        }

        public async Task<IEnumerable<Competition>> SearchCompetitionsByNameAsync(string name)
        {
            try
            {
                _logger.LogInformation("Buscando competições por nome: {Name}", name);

                var competitions = await _context.Competitions
                    .Where(c => c.Name.ToLower().Contains(name.ToLower()))
                    .Include(c => c.Participants)
                        .ThenInclude(cp => cp.Participant)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Encontradas {Count} competições com nome contendo '{Name}'", competitions.Count, name);
                return competitions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar competições por nome: {Name}", name);
                throw;
            }
        }

        public async Task<Competition> UpdateCompetitionAsync(Competition competition)
        {
            try
            {
                _logger.LogInformation("Atualizando competição ID: {Id}", competition.Id);

                competition.UpdatedAt = DateTime.UtcNow;
                _context.Competitions.Update(competition);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Competição atualizada com sucesso. ID: {Id}", competition.Id);
                return competition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar competição ID: {Id}", competition.Id);
                throw;
            }
        }

        public async Task DeleteCompetitionAsync(int id)
        {
            try
            {
                _logger.LogInformation("Excluindo competição ID: {Id}", id);

                var competition = await _context.Competitions.FindAsync(id);
                if (competition == null)
                {
                    _logger.LogWarning("Competição não encontrada para exclusão. ID: {Id}", id);
                    throw new ArgumentException($"Competição com ID {id} não encontrada");
                }

                _context.Competitions.Remove(competition);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Competição excluída com sucesso. ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir competição ID: {Id}", id);
                throw;
            }
        }
    }

    public interface IParticipantService
    {
        Task<Participant> CreateParticipantAsync(string name, string email = "");
        Task<Participant?> GetParticipantByIdAsync(int id);
        Task<Participant?> GetParticipantByNameAsync(string name);
        Task<IEnumerable<Participant>> GetAllParticipantsAsync();
        Task<Participant> UpdateParticipantAsync(Participant participant);
        Task DeleteParticipantAsync(int id);
        Task<CompetitionParticipant> RegisterParticipantInCompetitionAsync(int competitionId, int participantId);
        Task<IEnumerable<Participant>> GetParticipantsByCompetitionAsync(int competitionId);
    }

    public class ParticipantService : IParticipantService
    {
        private readonly CompetitionDbContext _context;
        private readonly ILogger<ParticipantService> _logger;

        public ParticipantService(CompetitionDbContext context, ILogger<ParticipantService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Participant> CreateParticipantAsync(string name, string email = "")
        {
            try
            {
                _logger.LogInformation("Criando novo participante: {Name}", name);

                // Verificar se já existe
                var existingParticipant = await GetParticipantByNameAsync(name);
                if (existingParticipant != null)
                {
                    _logger.LogInformation("Participante já existe: {Name}", name);
                    return existingParticipant;
                }

                var participant = new Participant
                {
                    Name = name,
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Participants.Add(participant);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Participante criado com sucesso. ID: {Id}", participant.Id);
                return participant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar participante: {Name}", name);
                throw;
            }
        }

        public async Task<Participant?> GetParticipantByIdAsync(int id)
        {
            try
            {
                return await _context.Participants.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar participante por ID: {Id}", id);
                throw;
            }
        }

        public async Task<Participant?> GetParticipantByNameAsync(string name)
        {
            try
            {
                return await _context.Participants
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar participante por nome: {Name}", name);
                throw;
            }
        }

        public async Task<IEnumerable<Participant>> GetAllParticipantsAsync()
        {
            try
            {
                return await _context.Participants
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os participantes");
                throw;
            }
        }

        public async Task<Participant> UpdateParticipantAsync(Participant participant)
        {
            try
            {
                participant.UpdatedAt = DateTime.UtcNow;
                _context.Participants.Update(participant);
                await _context.SaveChangesAsync();
                return participant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar participante ID: {Id}", participant.Id);
                throw;
            }
        }

        public async Task DeleteParticipantAsync(int id)
        {
            try
            {
                var participant = await _context.Participants.FindAsync(id);
                if (participant == null)
                {
                    throw new ArgumentException($"Participante com ID {id} não encontrado");
                }

                _context.Participants.Remove(participant);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir participante ID: {Id}", id);
                throw;
            }
        }

        public async Task<CompetitionParticipant> RegisterParticipantInCompetitionAsync(int competitionId, int participantId)
        {
            try
            {
                // Verificar se já está registrado
                var existing = await _context.CompetitionParticipants
                    .FirstOrDefaultAsync(cp => cp.CompetitionId == competitionId && cp.ParticipantId == participantId);

                if (existing != null)
                {
                    return existing;
                }

                var registration = new CompetitionParticipant
                {
                    CompetitionId = competitionId,
                    ParticipantId = participantId,
                    RegisteredAt = DateTime.UtcNow
                };

                _context.CompetitionParticipants.Add(registration);
                await _context.SaveChangesAsync();

                return registration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar participante {ParticipantId} na competição {CompetitionId}", participantId, competitionId);
                throw;
            }
        }

        public async Task<IEnumerable<Participant>> GetParticipantsByCompetitionAsync(int competitionId)
        {
            try
            {
                return await _context.CompetitionParticipants
                    .Where(cp => cp.CompetitionId == competitionId)
                    .Include(cp => cp.Participant)
                    .Select(cp => cp.Participant)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar participantes da competição {CompetitionId}", competitionId);
                throw;
            }
        }
    }
}

