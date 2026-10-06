-- TECH4HR
-- SCRIPT 04: OPERACIONAL PASSA A SER UM TIPO DE FUNCIONARIO
-- Banco: Tech4HrDB
--
-- O operacional deixa de ter conta na tabela Usuario e passa a ser um
-- funcionario com NivelAcesso = 'OPERACIONAL'. Ele entra pelo login de
-- funcionario, bate ponto como os demais e ganha as telas de gestao.
--
-- E seguro rodar com a API antiga no ar: a coluna nasce com o valor padrao
-- 'FUNCIONARIO', entao nada muda para quem ja esta cadastrado.
-- Pode ser executado mais de uma vez.
--
-- ORDEM: rode este script ANTES de publicar a API nova.

-- =========================================
-- 1. COLUNA NivelAcesso
-- =========================================

IF COL_LENGTH('dbo.Funcionario', 'NivelAcesso') IS NULL
BEGIN
    ALTER TABLE dbo.Funcionario
        ADD NivelAcesso VARCHAR(20) NOT NULL
            CONSTRAINT DF_Funcionario_NivelAcesso
            DEFAULT 'FUNCIONARIO';
END;
GO

-- =========================================
-- 2. REGRA: SO DOIS NIVEIS
-- =========================================

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Funcionario_NivelAcesso'
      AND parent_object_id = OBJECT_ID('dbo.Funcionario')
)
BEGIN
    ALTER TABLE dbo.Funcionario
        ADD CONSTRAINT CK_Funcionario_NivelAcesso
        CHECK (NivelAcesso IN ('FUNCIONARIO', 'OPERACIONAL'));
END;
GO

-- =========================================
-- 3. VERIFICACAO
-- =========================================

SELECT
    NivelAcesso,
    COUNT(*) AS Total
FROM dbo.Funcionario
GROUP BY NivelAcesso;
GO

-- Usuarios OPERACIONAL antigos. A conta deles na tabela Usuario nao entra
-- mais pelo login administrativo. Para cada um, um ADMIN deve cadastrar o
-- mesmo e-mail como funcionario com nivel OPERACIONAL (CPF e data de
-- admissao nao existem na tabela Usuario, por isso nao ha migracao
-- automatica). Depois a conta antiga pode ser desativada.
SELECT
    IdUsuario,
    Nome,
    Sobrenome,
    Email,
    Ativo
FROM dbo.Usuario
WHERE NivelUsuario = 'OPERACIONAL'
ORDER BY Nome;
GO
