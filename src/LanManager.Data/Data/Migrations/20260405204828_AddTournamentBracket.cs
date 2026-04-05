using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentBracket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Some dev DBs may already have these tables (from prior runs). Make this migration safe to apply.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Tournaments]', N'U') IS NULL
BEGIN
    CREATE TABLE [Tournaments] (
        [Id] uniqueidentifier NOT NULL,
        [EventId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Format] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Tournaments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tournaments_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'[TournamentMatches]', N'U') IS NULL
BEGIN
    CREATE TABLE [TournamentMatches] (
        [Id] uniqueidentifier NOT NULL,
        [TournamentId] uniqueidentifier NOT NULL,
        [Round] int NOT NULL,
        [MatchNumber] int NOT NULL,
        [Player1Id] uniqueidentifier NULL,
        [Player1Name] nvarchar(max) NULL,
        [Player2Id] uniqueidentifier NULL,
        [Player2Name] nvarchar(max) NULL,
        [WinnerId] uniqueidentifier NULL,
        [WinnerName] nvarchar(max) NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_TournamentMatches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TournamentMatches_Tournaments_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments] ([Id]) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'[TournamentParticipants]', N'U') IS NULL
BEGIN
    CREATE TABLE [TournamentParticipants] (
        [Id] uniqueidentifier NOT NULL,
        [TournamentId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [Seed] int NOT NULL,
        CONSTRAINT [PK_TournamentParticipants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TournamentParticipants_Tournaments_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments] ([Id]) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'[TournamentMatches]', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_TournamentMatches_TournamentId_Round_MatchNumber'
      AND object_id = OBJECT_ID(N'[TournamentMatches]')
)
BEGIN
    CREATE INDEX [IX_TournamentMatches_TournamentId_Round_MatchNumber]
    ON [TournamentMatches] ([TournamentId], [Round], [MatchNumber]);
END;

IF OBJECT_ID(N'[TournamentParticipants]', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_TournamentParticipants_TournamentId_UserId'
      AND object_id = OBJECT_ID(N'[TournamentParticipants]')
)
BEGIN
    CREATE UNIQUE INDEX [IX_TournamentParticipants_TournamentId_UserId]
    ON [TournamentParticipants] ([TournamentId], [UserId]);
END;

IF OBJECT_ID(N'[Tournaments]', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_Tournaments_EventId'
      AND object_id = OBJECT_ID(N'[Tournaments]')
)
BEGIN
    CREATE INDEX [IX_Tournaments_EventId]
    ON [Tournaments] ([EventId]);
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[TournamentMatches]', N'U') IS NOT NULL DROP TABLE [TournamentMatches];
IF OBJECT_ID(N'[TournamentParticipants]', N'U') IS NOT NULL DROP TABLE [TournamentParticipants];
IF OBJECT_ID(N'[Tournaments]', N'U') IS NOT NULL DROP TABLE [Tournaments];
");
        }
    }
}
