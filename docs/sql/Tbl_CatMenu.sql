-- Catálogo de menú lateral y permisos por rol.
-- La API también crea estas tablas al iniciar si no existen.

IF OBJECT_ID(N'dbo.Tbl_CatMenu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tbl_CatMenu (
        IdMenu INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        IdMenuPadre INT NULL,
        Codigo NVARCHAR(80) NOT NULL UNIQUE,
        Titulo NVARCHAR(120) NOT NULL,
        Controlador NVARCHAR(80) NULL,
        Accion NVARCHAR(80) NULL,
        IconoCss NVARCHAR(80) NULL,
        Tipo NVARCHAR(20) NOT NULL DEFAULT (N'Item'),
        Orden INT NOT NULL DEFAULT (0),
        Target NVARCHAR(20) NULL,
        UsaFechaHoy BIT NOT NULL DEFAULT (0),
        Activo BIT NOT NULL DEFAULT (1),
        CONSTRAINT FK_Tbl_CatMenu_Padre FOREIGN KEY (IdMenuPadre) REFERENCES dbo.Tbl_CatMenu (IdMenu)
    );
END
GO

IF OBJECT_ID(N'dbo.Tbl_RolMenu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tbl_RolMenu (
        IdRolMenu INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        IdRol INT NOT NULL,
        IdMenu INT NOT NULL,
        Activo BIT NOT NULL DEFAULT (1),
        CONSTRAINT UQ_Tbl_RolMenu UNIQUE (IdRol, IdMenu),
        CONSTRAINT FK_Tbl_RolMenu_Rol FOREIGN KEY (IdRol) REFERENCES dbo.Tbl_Rol (IdRol),
        CONSTRAINT FK_Tbl_RolMenu_Menu FOREIGN KEY (IdMenu) REFERENCES dbo.Tbl_CatMenu (IdMenu)
    );
END
GO
