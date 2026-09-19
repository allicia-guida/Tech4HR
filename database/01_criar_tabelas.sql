-- TECH4HR
-- SCRIPT 01: CRIACAO DAS TABELAS
-- Banco: Tech4HrDB

-- =========================================
-- 1. FUNCIONARIO
-- =========================================

CREATE TABLE dbo.Funcionario (
    IdFuncionario INT IDENTITY(1,1) NOT NULL,
    Nome NVARCHAR(100) NOT NULL,
    Sobrenome NVARCHAR(100) NOT NULL,
    EmailCorporativo NVARCHAR(255) NOT NULL,
    SenhaHash NVARCHAR(500) NOT NULL,
    DataAdmissao DATE NOT NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    CPF CHAR(11) NOT NULL,

    CONSTRAINT PK_Funcionario
        PRIMARY KEY (IdFuncionario),

    CONSTRAINT UQ_Funcionario_Email
        UNIQUE (EmailCorporativo),

    CONSTRAINT UQ_Funcionario_CPF
        UNIQUE (CPF),

    CONSTRAINT CK_Funcionario_CPF
        CHECK (
            LEN(CPF) = 11
            AND CPF NOT LIKE '%[^0-9]%'
        )
);
GO

-- =========================================
-- 2. USUARIO
-- =========================================

CREATE TABLE dbo.Usuario (
    IdUsuario INT IDENTITY(1,1) NOT NULL,
    Nome NVARCHAR(100) NOT NULL,
    Sobrenome NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    SenhaHash NVARCHAR(500) NOT NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    NivelUsuario VARCHAR(20) NOT NULL,

    CONSTRAINT PK_Usuario
        PRIMARY KEY (IdUsuario),

    CONSTRAINT UQ_Usuario_Email
        UNIQUE (Email),

    CONSTRAINT CK_Usuario_Nivel
        CHECK (
            NivelUsuario IN ('OPERACIONAL', 'ADMIN')
        )
);
GO

-- =========================================
-- 3. PONTO (ESPELHO DO EXPEDIENTE)
-- =========================================

CREATE TABLE dbo.Ponto (
    IdPonto INT IDENTITY(1,1) NOT NULL,
    IdFuncionario INT NOT NULL,
    DataPonto DATE NOT NULL,

    Entrada DATETIME2(0) NULL,
    SaidaAlmoco DATETIME2(0) NULL,
    EntradaAlmoco DATETIME2(0) NULL,
    Saida DATETIME2(0) NULL,

    CONSTRAINT PK_Ponto
        PRIMARY KEY (IdPonto),

    CONSTRAINT FK_Ponto_Funcionario
        FOREIGN KEY (IdFuncionario)
        REFERENCES dbo.Funcionario(IdFuncionario),

    CONSTRAINT UQ_Ponto_Funcionario_Data
        UNIQUE (IdFuncionario, DataPonto),

    CONSTRAINT CK_Ponto_Ordem
        CHECK (
            (SaidaAlmoco IS NULL OR
                (Entrada IS NOT NULL
                 AND SaidaAlmoco >= Entrada))
            AND
            (EntradaAlmoco IS NULL OR
                (SaidaAlmoco IS NOT NULL
                 AND EntradaAlmoco >= SaidaAlmoco))
            AND
            (Saida IS NULL OR
                (EntradaAlmoco IS NOT NULL
                 AND Saida >= EntradaAlmoco))
        )
);
GO

-- =========================================
-- 4. REGISTRO DE PONTO
-- =========================================

CREATE TABLE dbo.RegistroPonto (
    IdRegistroPonto BIGINT IDENTITY(1,1) NOT NULL,
    IdFuncionario INT NOT NULL,
    IdPonto INT NOT NULL,

    TipoRegistro VARCHAR(20) NOT NULL,
    DataHora DATETIMEOFFSET(0) NOT NULL,
    DataHoraRecebimento DATETIMEOFFSET(0)
        NOT NULL DEFAULT SYSDATETIMEOFFSET(),

    CONSTRAINT PK_RegistroPonto
        PRIMARY KEY (IdRegistroPonto),

    CONSTRAINT FK_RegistroPonto_Funcionario
        FOREIGN KEY (IdFuncionario)
        REFERENCES dbo.Funcionario(IdFuncionario),

    CONSTRAINT FK_RegistroPonto_Ponto
        FOREIGN KEY (IdPonto)
        REFERENCES dbo.Ponto(IdPonto),

    CONSTRAINT CK_RegistroPonto_Tipo
        CHECK (
            TipoRegistro IN (
                'ENTRADA',
                'SAIDA_ALMOCO',
                'ENTRADA_ALMOCO',
                'SAIDA'
            )
        )
);
GO

-- =========================================
-- 5. INDICES
-- =========================================

CREATE INDEX IX_RegistroPonto_Funcionario_Data
ON dbo.RegistroPonto (
    IdFuncionario,
    DataHora
);
GO

CREATE UNIQUE INDEX UX_RegistroPonto_Ponto_Tipo
ON dbo.RegistroPonto (
    IdPonto,
    TipoRegistro
);
GO

-- =========================================
-- 6. VERIFICACAO
-- =========================================

SELECT
    TABLE_NAME AS NomeTabela
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_TYPE = 'BASE TABLE'
  AND TABLE_NAME IN (
      'Funcionario',
      'Usuario',
      'Ponto',
      'RegistroPonto'
  )
ORDER BY TABLE_NAME;
GO