-- Script de inicialização do banco de dados PostgreSQL
-- Execute este script após criar o banco de dados no Azure

-- Conectar ao banco de dados competitiondb
\c competitiondb;

-- Criar extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Verificar se as tabelas já existem antes de criar
DO $$
BEGIN
    -- Criar tabela de competições se não existir
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'competitions') THEN
        CREATE TABLE competitions (
            id SERIAL PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            description VARCHAR(1000),
            competition_date TIMESTAMP WITH TIME ZONE NOT NULL,
            created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
            updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
        );
        
        CREATE INDEX ix_competitions_created_at ON competitions(created_at);
        
        RAISE NOTICE 'Tabela competitions criada com sucesso';
    ELSE
        RAISE NOTICE 'Tabela competitions já existe';
    END IF;

    -- Criar tabela de participantes se não existir
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'participants') THEN
        CREATE TABLE participants (
            id SERIAL PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            email VARCHAR(200),
            created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
            updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
        );
        
        CREATE UNIQUE INDEX ix_participants_name ON participants(LOWER(name));
        
        RAISE NOTICE 'Tabela participants criada com sucesso';
    ELSE
        RAISE NOTICE 'Tabela participants já existe';
    END IF;

    -- Criar tabela de relacionamento competição-participante se não existir
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'competition_participants') THEN
        CREATE TABLE competition_participants (
            id SERIAL PRIMARY KEY,
            competition_id INTEGER NOT NULL,
            participant_id INTEGER NOT NULL,
            registered_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (competition_id) REFERENCES competitions(id) ON DELETE CASCADE,
            FOREIGN KEY (participant_id) REFERENCES participants(id) ON DELETE CASCADE
        );
        
        CREATE UNIQUE INDEX ix_competition_participants_unique ON competition_participants(competition_id, participant_id);
        
        RAISE NOTICE 'Tabela competition_participants criada com sucesso';
    ELSE
        RAISE NOTICE 'Tabela competition_participants já existe';
    END IF;

    -- Criar tabela de resultados se não existir
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'results') THEN
        CREATE TABLE results (
            id SERIAL PRIMARY KEY,
            competition_id INTEGER NOT NULL,
            participant_id INTEGER NOT NULL,
            round_number INTEGER NOT NULL,
            time_in_seconds DECIMAL(10,3) NOT NULL DEFAULT 0,
            alfa_count INTEGER NOT NULL DEFAULT 0,
            bravo_count INTEGER NOT NULL DEFAULT 0,
            charlie_count INTEGER NOT NULL DEFAULT 0,
            miss_count INTEGER NOT NULL DEFAULT 0,
            fault_count INTEGER NOT NULL DEFAULT 0,
            vitima_count INTEGER NOT NULL DEFAULT 0,
            plate_count INTEGER NOT NULL DEFAULT 0,
            total_time DECIMAL(10,3) NOT NULL DEFAULT 0,
            is_eliminated BOOLEAN NOT NULL DEFAULT FALSE,
            created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
            updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (competition_id) REFERENCES competitions(id) ON DELETE CASCADE,
            FOREIGN KEY (participant_id) REFERENCES participants(id) ON DELETE CASCADE
        );
        
        CREATE UNIQUE INDEX ix_results_unique ON results(competition_id, participant_id, round_number);
        CREATE INDEX ix_results_competition_id ON results(competition_id);
        
        RAISE NOTICE 'Tabela results criada com sucesso';
    ELSE
        RAISE NOTICE 'Tabela results já existe';
    END IF;

    -- Criar tabela de resultados finais se não existir
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'final_results') THEN
        CREATE TABLE final_results (
            id SERIAL PRIMARY KEY,
            competition_id INTEGER NOT NULL,
            participant_id INTEGER NOT NULL,
            position INTEGER NOT NULL,
            round1_time DECIMAL(10,3) NOT NULL DEFAULT 0,
            round2_time DECIMAL(10,3) NOT NULL DEFAULT 0,
            best_time DECIMAL(10,3) NOT NULL DEFAULT 0,
            best_round INTEGER NOT NULL DEFAULT 0,
            created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
            updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (competition_id) REFERENCES competitions(id) ON DELETE CASCADE,
            FOREIGN KEY (participant_id) REFERENCES participants(id) ON DELETE CASCADE
        );
        
        CREATE UNIQUE INDEX ix_final_results_unique ON final_results(competition_id, participant_id);
        CREATE INDEX ix_final_results_competition_id ON final_results(competition_id);
        
        RAISE NOTICE 'Tabela final_results criada com sucesso';
    ELSE
        RAISE NOTICE 'Tabela final_results já existe';
    END IF;
END
$$;

-- Criar função para atualizar timestamp automaticamente
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Criar triggers para atualizar updated_at automaticamente
DO $$
BEGIN
    -- Trigger para competitions
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_competitions_updated_at') THEN
        CREATE TRIGGER update_competitions_updated_at
            BEFORE UPDATE ON competitions
            FOR EACH ROW
            EXECUTE FUNCTION update_updated_at_column();
        RAISE NOTICE 'Trigger update_competitions_updated_at criado';
    END IF;

    -- Trigger para participants
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_participants_updated_at') THEN
        CREATE TRIGGER update_participants_updated_at
            BEFORE UPDATE ON participants
            FOR EACH ROW
            EXECUTE FUNCTION update_updated_at_column();
        RAISE NOTICE 'Trigger update_participants_updated_at criado';
    END IF;

    -- Trigger para results
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_results_updated_at') THEN
        CREATE TRIGGER update_results_updated_at
            BEFORE UPDATE ON results
            FOR EACH ROW
            EXECUTE FUNCTION update_updated_at_column();
        RAISE NOTICE 'Trigger update_results_updated_at criado';
    END IF;

    -- Trigger para final_results
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_final_results_updated_at') THEN
        CREATE TRIGGER update_final_results_updated_at
            BEFORE UPDATE ON final_results
            FOR EACH ROW
            EXECUTE FUNCTION update_updated_at_column();
        RAISE NOTICE 'Trigger update_final_results_updated_at criado';
    END IF;
END
$$;

-- Inserir dados de exemplo (opcional)
INSERT INTO competitions (name, description, competition_date) 
VALUES ('Competição de Exemplo', 'Competição criada automaticamente para teste', CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

-- Verificar se as tabelas foram criadas corretamente
SELECT 
    schemaname,
    tablename,
    tableowner
FROM pg_tables 
WHERE schemaname = 'public' 
ORDER BY tablename;

-- Exibir estatísticas das tabelas
SELECT 
    t.table_name,
    COALESCE(c.row_count, 0) as row_count
FROM information_schema.tables t
LEFT JOIN (
    SELECT 
        schemaname||'.'||tablename as table_name,
        n_tup_ins - n_tup_del as row_count
    FROM pg_stat_user_tables
) c ON t.table_schema||'.'||t.table_name = c.table_name
WHERE t.table_schema = 'public'
ORDER BY t.table_name;

RAISE NOTICE 'Script de inicialização executado com sucesso!';

