IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [NombreCompleto] nvarchar(max) NOT NULL,
    [Telefono] nvarchar(max) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Categorias] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [EstaActiva] bit NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Estilistas] (
    [Id] int NOT NULL IDENTITY,
    [IdentityId] nvarchar(max) NOT NULL,
    [NombreCompleto] nvarchar(max) NOT NULL,
    [Telefono] nvarchar(max) NOT NULL,
    [EstaActivo] bit NOT NULL,
    [Imagen] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Estilistas] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ParametrosSistema] (
    [Id] int NOT NULL IDENTITY,
    [BufferMinutos] int NOT NULL,
    [ToleranciaLlegadaMinutos] int NOT NULL,
    [DuracionMinimaServicioMinutos] int NOT NULL,
    CONSTRAINT [PK_ParametrosSistema] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Servicios] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [Descripcion] nvarchar(max) NOT NULL,
    [DuracionMinutos] int NOT NULL,
    [Precio] decimal(18,2) NOT NULL,
    [Imagen] nvarchar(max) NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    [Disponible] bit NOT NULL,
    [CategoriaId] int NOT NULL,
    CONSTRAINT [PK_Servicios] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Servicios_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [BloqueosDescansoFijoDiario] (
    [Id] int NOT NULL IDENTITY,
    [EstilistaId] int NOT NULL,
    [DiaSemana] int NOT NULL,
    [HoraInicioDescanso] time NOT NULL,
    [HoraFinDescanso] time NOT NULL,
    [Razon] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_BloqueosDescansoFijoDiario] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BloqueosDescansoFijoDiario_Estilistas_EstilistaId] FOREIGN KEY ([EstilistaId]) REFERENCES [Estilistas] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [BloqueosRangoDiasLibres] (
    [Id] int NOT NULL IDENTITY,
    [EstilistaId] int NOT NULL,
    [FechaInicioBloqueo] datetime2 NOT NULL,
    [FechaFinBloqueo] datetime2 NOT NULL,
    [Razon] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_BloqueosRangoDiasLibres] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BloqueosRangoDiasLibres_Estilistas_EstilistaId] FOREIGN KEY ([EstilistaId]) REFERENCES [Estilistas] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [HorariosSemanalBase] (
    [Id] int NOT NULL IDENTITY,
    [EstilistaId] int NOT NULL,
    [DiaSemana] int NOT NULL,
    [HoraInicioJornada] time NOT NULL,
    [HoraFinJornada] time NOT NULL,
    [EsLaborable] bit NOT NULL,
    CONSTRAINT [PK_HorariosSemanalBase] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HorariosSemanalBase_Estilistas_EstilistaId] FOREIGN KEY ([EstilistaId]) REFERENCES [Estilistas] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [EstilistaServicios] (
    [EstilistaId] int NOT NULL,
    [ServicioId] int NOT NULL,
    CONSTRAINT [PK_EstilistaServicios] PRIMARY KEY ([EstilistaId], [ServicioId]),
    CONSTRAINT [FK_EstilistaServicios_Estilistas_EstilistaId] FOREIGN KEY ([EstilistaId]) REFERENCES [Estilistas] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_EstilistaServicios_Servicios_ServicioId] FOREIGN KEY ([ServicioId]) REFERENCES [Servicios] ([Id]) ON DELETE CASCADE
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
    SET IDENTITY_INSERT [AspNetRoles] ON;
INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
VALUES (N'a20b1a03-9f2d-45f8-8f8e-20bc8b67b7e3', NULL, N'Estilista', N'ESTILISTA'),
(N'd17abceb-8c0b-454e-b296-883bc029d82b', NULL, N'Admin', N'ADMIN'),
(N'e10b1a03-9f2d-45f8-8f8e-20bc8b67b7e3', NULL, N'Cliente', N'CLIENTE');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
    SET IDENTITY_INSERT [AspNetRoles] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessFailedCount', N'ConcurrencyStamp', N'Email', N'EmailConfirmed', N'LockoutEnabled', N'LockoutEnd', N'NombreCompleto', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'PhoneNumber', N'PhoneNumberConfirmed', N'SecurityStamp', N'Telefono', N'TwoFactorEnabled', N'UserName') AND [object_id] = OBJECT_ID(N'[AspNetUsers]'))
    SET IDENTITY_INSERT [AspNetUsers] ON;
INSERT INTO [AspNetUsers] ([Id], [AccessFailedCount], [ConcurrencyStamp], [Email], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NombreCompleto], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [Telefono], [TwoFactorEnabled], [UserName])
VALUES (N'a18be9c0-aa65-4af8-bd17-00bd9344e575', 0, N'00000000-0000-0000-0000-000000000000', N'admin@test.com', CAST(1 AS bit), CAST(0 AS bit), NULL, N'Administrador Principal', N'ADMIN@TEST.COM', N'ADMIN', N'AQAAAAIAAYagAAAAEN+hPrKzQYsYTBIqnqg2hF43tujnNomeFYN/J+S1Jz+zy2nYmHP5ulYoVYBcqgoZew==', NULL, CAST(0 AS bit), N'00000000-0000-0000-0000-000000000000', N'3001234567', CAST(0 AS bit), N'admin'),
(N'b7e289d1-d21a-4c9f-8d7e-00bd9344e575', 0, N'a6b6ef85-033b-4509-aa76-8aa802cce0ef', N'laura.e@pelu.com', CAST(1 AS bit), CAST(0 AS bit), NULL, N'Laura Valencia', N'LAURA.E@PELU.COM', N'LAURA.E', N'AQAAAAIAAYagAAAAEDVTXjCykFmuLJGxA9W4xNRKB6kg6vZf91/eBwoJtuhu0Hg8z2TZxloArwgcpTHjMg==', NULL, CAST(0 AS bit), N'99dd5ddb-2818-461c-8b42-8c3127f02bd6', N'3001234568', CAST(0 AS bit), N'laura.e'),
(N'c7e289d1-d21a-4c9f-8d7e-00bd9344e575', 0, N'ceb389af-3d15-4aca-b589-c0c08c23ac0e', N'juan.c@mail.com', CAST(1 AS bit), CAST(0 AS bit), NULL, N'Juan Cliente', N'JUAN.C@MAIL.COM', N'JUAN.C', N'AQAAAAIAAYagAAAAEKONFU6ObDjEBACKQqH/rVBTW77r5MFosZbrd0KqGeYBbvCDmgxTg7HjZhCIEL2BoQ==', NULL, CAST(0 AS bit), N'51b48321-c9a5-49ef-b322-312eb25afc20', N'3109876543', CAST(0 AS bit), N'juan.c');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessFailedCount', N'ConcurrencyStamp', N'Email', N'EmailConfirmed', N'LockoutEnabled', N'LockoutEnd', N'NombreCompleto', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'PhoneNumber', N'PhoneNumberConfirmed', N'SecurityStamp', N'Telefono', N'TwoFactorEnabled', N'UserName') AND [object_id] = OBJECT_ID(N'[AspNetUsers]'))
    SET IDENTITY_INSERT [AspNetUsers] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'EstaActiva', N'Nombre') AND [object_id] = OBJECT_ID(N'[Categorias]'))
    SET IDENTITY_INSERT [Categorias] ON;
INSERT INTO [Categorias] ([Id], [EstaActiva], [Nombre])
VALUES (1, CAST(1 AS bit), N'Cortes de Cabello'),
(2, CAST(1 AS bit), N'Tratamientos Capilares'),
(3, CAST(1 AS bit), N'Manicura y Pedicura'),
(4, CAST(1 AS bit), N'Otra');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'EstaActiva', N'Nombre') AND [object_id] = OBJECT_ID(N'[Categorias]'))
    SET IDENTITY_INSERT [Categorias] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'EstaActivo', N'IdentityId', N'Imagen', N'NombreCompleto', N'Telefono') AND [object_id] = OBJECT_ID(N'[Estilistas]'))
    SET IDENTITY_INSERT [Estilistas] ON;
INSERT INTO [Estilistas] ([Id], [EstaActivo], [IdentityId], [Imagen], [NombreCompleto], [Telefono])
VALUES (1, CAST(1 AS bit), N'a18be9c0-aa65-4af8-bd17-00bd9344e575', N'', N'Administrador Principal', N'3001234567'),
(2, CAST(1 AS bit), N'b7e289d1-d21a-4c9f-8d7e-00bd9344e575', N'', N'Laura Valencia', N'3001234568');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'EstaActivo', N'IdentityId', N'Imagen', N'NombreCompleto', N'Telefono') AND [object_id] = OBJECT_ID(N'[Estilistas]'))
    SET IDENTITY_INSERT [Estilistas] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BufferMinutos', N'DuracionMinimaServicioMinutos', N'ToleranciaLlegadaMinutos') AND [object_id] = OBJECT_ID(N'[ParametrosSistema]'))
    SET IDENTITY_INSERT [ParametrosSistema] ON;
INSERT INTO [ParametrosSistema] ([Id], [BufferMinutos], [DuracionMinimaServicioMinutos], [ToleranciaLlegadaMinutos])
VALUES (1, 5, 45, 10);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BufferMinutos', N'DuracionMinimaServicioMinutos', N'ToleranciaLlegadaMinutos') AND [object_id] = OBJECT_ID(N'[ParametrosSistema]'))
    SET IDENTITY_INSERT [ParametrosSistema] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[AspNetUserRoles]'))
    SET IDENTITY_INSERT [AspNetUserRoles] ON;
INSERT INTO [AspNetUserRoles] ([RoleId], [UserId])
VALUES (N'd17abceb-8c0b-454e-b296-883bc029d82b', N'a18be9c0-aa65-4af8-bd17-00bd9344e575'),
(N'a20b1a03-9f2d-45f8-8f8e-20bc8b67b7e3', N'b7e289d1-d21a-4c9f-8d7e-00bd9344e575'),
(N'e10b1a03-9f2d-45f8-8f8e-20bc8b67b7e3', N'c7e289d1-d21a-4c9f-8d7e-00bd9344e575');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[AspNetUserRoles]'))
    SET IDENTITY_INSERT [AspNetUserRoles] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'DiaSemana', N'EstilistaId', N'HoraFinDescanso', N'HoraInicioDescanso', N'Razon') AND [object_id] = OBJECT_ID(N'[BloqueosDescansoFijoDiario]'))
    SET IDENTITY_INSERT [BloqueosDescansoFijoDiario] ON;
INSERT INTO [BloqueosDescansoFijoDiario] ([Id], [DiaSemana], [EstilistaId], [HoraFinDescanso], [HoraInicioDescanso], [Razon])
VALUES (1, 1, 2, '14:00:00', '13:00:00', N'Almuerzo'),
(2, 3, 2, '14:00:00', '13:00:00', N'Almuerzo'),
(3, 5, 2, '14:00:00', '13:00:00', N'Almuerzo');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'DiaSemana', N'EstilistaId', N'HoraFinDescanso', N'HoraInicioDescanso', N'Razon') AND [object_id] = OBJECT_ID(N'[BloqueosDescansoFijoDiario]'))
    SET IDENTITY_INSERT [BloqueosDescansoFijoDiario] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'EstilistaId', N'FechaFinBloqueo', N'FechaInicioBloqueo', N'Razon') AND [object_id] = OBJECT_ID(N'[BloqueosRangoDiasLibres]'))
    SET IDENTITY_INSERT [BloqueosRangoDiasLibres] ON;
INSERT INTO [BloqueosRangoDiasLibres] ([Id], [EstilistaId], [FechaFinBloqueo], [FechaInicioBloqueo], [Razon])
VALUES (1, 2, '2026-01-18T00:00:00.0000000', '2026-01-15T00:00:00.0000000', N'Vacaciones');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'EstilistaId', N'FechaFinBloqueo', N'FechaInicioBloqueo', N'Razon') AND [object_id] = OBJECT_ID(N'[BloqueosRangoDiasLibres]'))
    SET IDENTITY_INSERT [BloqueosRangoDiasLibres] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'DiaSemana', N'EsLaborable', N'EstilistaId', N'HoraFinJornada', N'HoraInicioJornada') AND [object_id] = OBJECT_ID(N'[HorariosSemanalBase]'))
    SET IDENTITY_INSERT [HorariosSemanalBase] ON;
INSERT INTO [HorariosSemanalBase] ([Id], [DiaSemana], [EsLaborable], [EstilistaId], [HoraFinJornada], [HoraInicioJornada])
VALUES (1, 1, CAST(1 AS bit), 2, '18:00:00', '09:00:00'),
(2, 3, CAST(1 AS bit), 2, '18:00:00', '09:00:00'),
(3, 5, CAST(1 AS bit), 2, '18:00:00', '09:00:00');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'DiaSemana', N'EsLaborable', N'EstilistaId', N'HoraFinJornada', N'HoraInicioJornada') AND [object_id] = OBJECT_ID(N'[HorariosSemanalBase]'))
    SET IDENTITY_INSERT [HorariosSemanalBase] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CategoriaId', N'Descripcion', N'Disponible', N'DuracionMinutos', N'FechaCreacion', N'Imagen', N'Nombre', N'Precio') AND [object_id] = OBJECT_ID(N'[Servicios]'))
    SET IDENTITY_INSERT [Servicios] ON;
INSERT INTO [Servicios] ([Id], [CategoriaId], [Descripcion], [Disponible], [DuracionMinutos], [FechaCreacion], [Imagen], [Nombre], [Precio])
VALUES (1, 1, N'Un corte clásico...', CAST(1 AS bit), 60, '2025-10-23T00:00:00.0000000', N'https://localhost:7274/images/bobCut.png', N'Corte Estilo Bob', 50000.0),
(2, 2, N'Tratamiento keratina...', CAST(1 AS bit), 90, '2025-10-23T00:00:00.0000000', N'https://localhost:7274/images/hidratacion.jpg', N'Hidratación Profunda', 80000.0),
(3, 3, N'Experiencia relajante...', CAST(1 AS bit), 75, '2025-10-23T00:00:00.0000000', N'https://localhost:7274/images/manicura.jpg', N'Manicura SPA Completa', 80000.0),
(4, 1, N'Corte clásico...', CAST(1 AS bit), 45, '2025-10-23T00:00:00.0000000', N'https://localhost:7274/images/manCut.png', N'Corte para hombre', 30000.0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CategoriaId', N'Descripcion', N'Disponible', N'DuracionMinutos', N'FechaCreacion', N'Imagen', N'Nombre', N'Precio') AND [object_id] = OBJECT_ID(N'[Servicios]'))
    SET IDENTITY_INSERT [Servicios] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EstilistaId', N'ServicioId') AND [object_id] = OBJECT_ID(N'[EstilistaServicios]'))
    SET IDENTITY_INSERT [EstilistaServicios] ON;
INSERT INTO [EstilistaServicios] ([EstilistaId], [ServicioId])
VALUES (1, 1),
(1, 2),
(1, 3),
(2, 1),
(2, 4);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EstilistaId', N'ServicioId') AND [object_id] = OBJECT_ID(N'[EstilistaServicios]'))
    SET IDENTITY_INSERT [EstilistaServicios] OFF;
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_BloqueosDescansoFijoDiario_EstilistaId] ON [BloqueosDescansoFijoDiario] ([EstilistaId]);
GO

CREATE INDEX [IX_BloqueosRangoDiasLibres_EstilistaId] ON [BloqueosRangoDiasLibres] ([EstilistaId]);
GO

CREATE INDEX [IX_EstilistaServicios_ServicioId] ON [EstilistaServicios] ([ServicioId]);
GO

CREATE INDEX [IX_HorariosSemanalBase_EstilistaId] ON [HorariosSemanalBase] ([EstilistaId]);
GO

CREATE INDEX [IX_Servicios_CategoriaId] ON [Servicios] ([CategoriaId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251201170752_InitialCreate', N'8.0.8');
GO

COMMIT;
GO

