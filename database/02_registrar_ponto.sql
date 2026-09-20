CREATE OR ALTER PROCEDURE dbo.sp_RegistrarPonto
    @IdFuncionario INT,
    @TipoRegistro VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Agora DATETIMEOFFSET(0) =
        SYSDATETIMEOFFSET();

    DECLARE @DataPonto DATE =
        CONVERT(DATE, @Agora);

    DECLARE @IdPonto INT;

    DECLARE @ProximoTipo VARCHAR(20);

    DECLARE @UltimoTipo VARCHAR(20);

    BEGIN TRY

        BEGIN TRANSACTION;

        -- Bloqueia o funcionário durante o registro,
        -- evitando batidas simultâneas para a mesma pessoa.

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Funcionario WITH (UPDLOCK, HOLDLOCK)
            WHERE IdFuncionario = @IdFuncionario
              AND Ativo = 1
        )
        BEGIN
            THROW 50001,
                'Funcionario inexistente ou inativo.',
                1;
        END;

        -- Procura o espelho do dia.

        SELECT @IdPonto = IdPonto
        FROM dbo.Ponto WITH (UPDLOCK, HOLDLOCK)
        WHERE IdFuncionario = @IdFuncionario
          AND DataPonto = @DataPonto;

        -- Cria um novo expediente somente
        -- quando a primeira batida for ENTRADA.

        IF @IdPonto IS NULL
        BEGIN

            IF @TipoRegistro <> 'ENTRADA'
            BEGIN
                THROW 50002,
                    'A primeira marcacao deve ser ENTRADA.',
                    1;
            END;

            INSERT INTO dbo.Ponto (
                IdFuncionario,
                DataPonto
            )
            VALUES (
                @IdFuncionario,
                @DataPonto
            );

            SET @IdPonto = SCOPE_IDENTITY();

        END;

        -- Identifica a ultima marcacao.

        SELECT TOP 1
            @UltimoTipo = TipoRegistro
        FROM dbo.RegistroPonto
        WHERE IdPonto = @IdPonto
        ORDER BY IdRegistroPonto DESC;

        -- Define a proxima marcacao permitida.

        SET @ProximoTipo =
            CASE
                WHEN @UltimoTipo IS NULL
                    THEN 'ENTRADA'

                WHEN @UltimoTipo = 'ENTRADA'
                    THEN 'SAIDA_ALMOCO'

                WHEN @UltimoTipo = 'SAIDA_ALMOCO'
                    THEN 'ENTRADA_ALMOCO'

                WHEN @UltimoTipo = 'ENTRADA_ALMOCO'
                    THEN 'SAIDA'

                ELSE NULL
            END;

        IF @ProximoTipo IS NULL
           OR @TipoRegistro <> @ProximoTipo
        BEGIN
            THROW 50003,
                'Marcacao invalida ou fora de sequencia.',
                1;
        END;

        -- Insere a batida original.

        INSERT INTO dbo.RegistroPonto (
            IdFuncionario,
            IdPonto,
            TipoRegistro,
            DataHora
        )
        VALUES (
            @IdFuncionario,
            @IdPonto,
            @TipoRegistro,
            @Agora
        );

        -- Atualiza o espelho do expediente.

        UPDATE dbo.Ponto
        SET
            Entrada =
                CASE
                    WHEN @TipoRegistro = 'ENTRADA'
                    THEN CAST(@Agora AS DATETIME2(0))
                    ELSE Entrada
                END,

            SaidaAlmoco =
                CASE
                    WHEN @TipoRegistro = 'SAIDA_ALMOCO'
                    THEN CAST(@Agora AS DATETIME2(0))
                    ELSE SaidaAlmoco
                END,

            EntradaAlmoco =
                CASE
                    WHEN @TipoRegistro = 'ENTRADA_ALMOCO'
                    THEN CAST(@Agora AS DATETIME2(0))
                    ELSE EntradaAlmoco
                END,

            Saida =
                CASE
                    WHEN @TipoRegistro = 'SAIDA'
                    THEN CAST(@Agora AS DATETIME2(0))
                    ELSE Saida
                END

        WHERE IdPonto = @IdPonto;

        COMMIT TRANSACTION;

        SELECT
            @IdPonto AS IdPonto,
            @TipoRegistro AS TipoRegistro,
            @Agora AS DataHora,
            'Ponto registrado com sucesso.' AS Mensagem;

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;

    END CATCH;

END;
GO