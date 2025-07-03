using CompetitionApp.Data;
using CompetitionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CompetitionApp.Services
{
    // Interfaces
    public interface ICompetitionService
    {
        Task<Competition> CreateCompetitionAsync(string name, string? description, DateTime competitionDate);
        Task<Competition?> GetCompetitionAsync(int id);
        Task<IEnumerable<Competition>> GetAllCompetitionsAsync();
        Task<Competition> UpdateCompetitionAsync(Competition competition);
        Task DeleteCompetitionAsync(int id);
    }

    public interface IParticipantService
    {
        Task<Participant> CreateParticipantAsync(string name, string? email = null);
        Task<Participant?> GetParticipantAsync(int id);
        Task<Participant?> GetParticipantByNameAsync(string name);
        Task<IEnumerable<Participant>> GetAllParticipantsAsync();
        Task<Participant> UpdateParticipantAsync(Participant participant);
        Task DeleteParticipantAsync(int id);
        Task<CompetitionParticipant> RegisterParticipantInCompetitionAsync(int competitionId, int participantId);
    }

    public interface IResultService
    {
        Task<Result> SaveResultAsync(int competitionId, int participantId, int roundNumber, decimal timeInSeconds, 
            int bravoCount, int charlieCount, int missCount, int faultCount, int vitimaCount, int plateCount, 
            decimal totalTime, bool isEliminated);
        Task<Result?> GetResultAsync(int competitionId, int participantId, int roundNumber);
        Task<IEnumerable<Result>> GetResultsByCompetitionIdAsync(int competitionId);
        Task<IEnumerable<Result>> GetResultsByParticipantIdAsync(int participantId);
        Task<IEnumerable<Result>> GetResultsByCompetitionAndRoundAsync(int competitionId, int roundNumber);
        Task DeleteResultAsync(int id);
        Task DeleteResultsByCompetitionAsync(int competitionId);
    }

    public interface IFinalResultService
    {
        Task<IEnumerable<FinalResult>> CalculateAndSaveFinalResultsAsync(int competitionId);
        Task<FinalResult?> GetFinalResultAsync(int competitionId, int participantId);
        Task<IEnumerable<FinalResult>> GetFinalResultsByCompetitionIdAsync(int competitionId);
        Task DeleteFinalResultAsync(int id);
        Task DeleteFinalResultsByCompetitionAsync(int competitionId);
    }

    // Implementações
    public class CompetitionService : ICompetitionService
    {
        private readonly CompetitionDbContext _context;
        private readonly ILogger<CompetitionService> _logger;

        public CompetitionService(CompetitionDbContext context, ILogger<CompetitionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Competition> CreateCompetitionAsync(string name, string? description, DateTime competitionDate)
        {
            try
            {
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

                _logger.LogInformation("Competição criada: {Name} (ID: {Id})", name, competition.Id);
                return competition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar competição: {Name}", name);
                throw;
            }
        }

        public async Task<Competition?> GetCompetitionAsync(int id)
        {
            try
            {
                return await _context.Competitions
                    .Include(c => c.CompetitionParticipants)
                    .ThenInclude(cp => cp.Participant)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar competição ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Competition>> GetAllCompetitionsAsync()
        {
            try
            {
                return await _context.Competitions
                    .Include(c => c.CompetitionParticipants)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todas as competições");
                throw;
            }
        }

        public async Task<Competition> UpdateCompetitionAsync(Competition competition)
        {
            try
            {
                competition.UpdatedAt = DateTime.UtcNow;
                _context.Competitions.Update(competition);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Competição atualizada: {Name} (ID: {Id})", competition.Name, competition.Id);
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
                var competition = await _context.Competitions.FindAsync(id);
                if (competition == null)
                {
                    throw new ArgumentException($"Competição com ID {id} não encontrada");
                }

                _context.Competitions.Remove(competition);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Competição excluída: {Name} (ID: {Id})", competition.Name, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir competição ID: {Id}", id);
                throw;
            }
        }
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

        public async Task<Participant> CreateParticipantAsync(string name, string? email = null)
        {
            try
            {
                var existing = await GetParticipantByNameAsync(name);
                if (existing != null)
                {
                    return existing;
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

                _logger.LogInformation("Participante criado: {Name} (ID: {Id})", name, participant.Id);
                return participant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar participante: {Name}", name);
                throw;
            }
        }

        public async Task<Participant?> GetParticipantAsync(int id)
        {
            try
            {
                return await _context.Participants.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar participante ID: {Id}", id);
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

                _logger.LogInformation("Participante atualizado: {Name} (ID: {Id})", participant.Name, participant.Id);
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

                _logger.LogInformation("Participante excluído: {Name} (ID: {Id})", participant.Name, id);
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

                _logger.LogInformation("Participante {ParticipantId} registrado na competição {CompetitionId}", 
                    participantId, competitionId);
                return registration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar participante {ParticipantId} na competição {CompetitionId}", 
                    participantId, competitionId);
                throw;
            }
        }
    }
}

