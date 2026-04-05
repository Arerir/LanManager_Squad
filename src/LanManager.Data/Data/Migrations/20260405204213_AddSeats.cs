using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Some dev DBs already have Seats (from prior runs). Make this migration safe to apply.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Seats]', N'U') IS NULL
BEGIN
    CREATE TABLE [Seats] (
        [Id] uniqueidentifier NOT NULL,
        [EventId] uniqueidentifier NOT NULL,
        [Row] int NOT NULL,
        [Column] int NOT NULL,
        [Label] nvarchar(max) NOT NULL,
        [AssignedUserId] uniqueidentifier NULL,
        [AssignedUserName] nvarchar(max) NULL,
        [AssignedAt] datetime2 NULL,
        CONSTRAINT [PK_Seats] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Seats_Events_EventId')
BEGIN
    ALTER TABLE [Seats] WITH CHECK ADD CONSTRAINT [FK_Seats_Events_EventId]
        FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Seats_EventId_Row_Column' AND object_id = OBJECT_ID(N'[Seats]')
)
BEGIN
    CREATE UNIQUE INDEX [IX_Seats_EventId_Row_Column] ON [Seats] ([EventId], [Row], [Column]);
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Seats]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [Seats];
END;
");
        }
    }
}
