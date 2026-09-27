-- TECH4HR
-- SCRIPT 03: TRIGGERS
-- Banco: Tech4HrDB

-- =========================================
-- IMPEDE A REMOCAO DO ULTIMO ADMIN ATIVO
-- =========================================

CREATE OR ALTER TRIGGER dbo.trg_Usuario_ManterAdminAtivo
ON dbo.Usuario
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Verifica se a alteração envolveu
    -- nível de usuário ou status.
    IF UPDATE(NivelUsuario) OR UPDATE(Ativo)
    BEGIN
        -- O sistema deve possuir pelo menos
        -- um administrador ativo.
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Usuario
            WHERE NivelUsuario = 'ADMIN'
              AND Ativo = 1
        )
        BEGIN
            ROLLBACK TRANSACTION;

            THROW 50010,
                'O sistema deve possuir pelo menos um administrador ativo.',
                1;
        END;
    END;
END;
GO