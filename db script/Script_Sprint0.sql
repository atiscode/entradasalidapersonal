USE RRHH
GO
/*****************  Tarea 1  ************************
	Proyecto: RRHH									*
	Fecha: 09/feb/2021
	Descripción: Empresa: Retirar del listado Finzo, 
		Evaluar , cambiar APS PTV por  Kickoff / outside
		box por OPINO
*****************************************************/

DELETE FROM adm.Catalogo WHERE NombreCatalogo IN ('FINZO')
UPDATE adm.Catalogo SET NombreCatalogo = 'KICKOFF', CodigoCatalogo = 'KICKOFF' WHERE CodigoCatalogo = 'PTV'
UPDATE adm.Catalogo SET NombreCatalogo = 'OPINO', CodigoCatalogo = 'OPINO' WHERE CodigoCatalogo = 'OSTB'
GO
/*****************  Tarea 3  ************************
	Proyecto: RRHH									*
	Fecha: 10/feb/2021
	Descripción: "Estado Civil: Aumentar en el catálogo 
	""Unión de Hecho"""
*****************************************************/
DECLARE @CatalogoPadre INT
SELECT @CatalogoPadre = IdCatalogo FROM adm.Catalogo WHERE CodigoCatalogo = 'ESTADO-CIVIL-01'
INSERT INTO adm.Catalogo (CodigoCatalogo, NombreCatalogo, DescripcionCatalogo, IdCatalogoPadre, EstadoCatalogo,IdEmpresa,Eliminado)
VALUES (NULL,'UNIÓN DE HECHO','UNIÓN DE HECHO', @CatalogoPadre, 1, 1, 0)
GO
/*****************  Tarea 18  ************************
	Proyecto: RRHH									*
	Fecha: 19/feb/2021
	Descripción: Referencia: Crear este nuevo campo 
		en Dirección (obligatorio)
*****************************************************/
ALTER TABLE FichaIngreso ADD Referencia VARCHAR(100)
GO

/*****************  Tarea 19  ************************
	Proyecto: RRHH									*
	Fecha: 19/feb/2021
	Descripción: Sector: Crear este nuevo campo 
		en Dirección (obligatorio)
*****************************************************/
ALTER TABLE FichaIngreso ADD Sector VARCHAR(100)
GO
/*****************  Tarea 25  ************************
	Proyecto: RRHH									*
	Fecha: 19/feb/2021
	Descripción: Tipo de Estudio: El Catálogo debe
		contener Bachiller/Tercer Nivel/Postgrado
*****************************************************/
DECLARE @CatalogoPadre INT
SELECT @CatalogoPadre= IdCatalogo FROM adm.Catalogo WHERE CodigoCatalogo = 'TIPO-ESTUDIO-01'


UPDATE adm.Catalogo SET NombreCatalogo = 'TERCER NIVEL', DescripcionCatalogo = 'TERCER NIVEL' 
WHERE IdCatalogoPadre = @CatalogoPadre AND NombreCatalogo = 'UNIVERSITARIO'

UPDATE adm.Catalogo SET NombreCatalogo = 'POSTGRADO', DescripcionCatalogo = 'POSTGRADO' 
WHERE IdCatalogoPadre = @CatalogoPadre AND NombreCatalogo = 'CUARTO NIVEL'
GO
/*****************  Tarea 25  ************************
	Proyecto: RRHH									*
	Fecha: 19/feb/2021
	Descripción: parentesco: Agregar nuevo campo 
	con una lista desplegable que contenga a 
	Cónyuge e Hijos
*****************************************************/


DECLARE @CodigoCatalogo VARCHAR(20) = 'CARGA-FAM'
INSERT INTO adm.Catalogo VALUES (@CodigoCatalogo, 'CARGAS FAMILIARES', 'CARGAS FAMILIARES',NULL, 1,1,0)

DECLARE @CatalogoPadre INT
SELECT @CatalogoPadre= IdCatalogo FROM adm.Catalogo WHERE CodigoCatalogo = @CodigoCatalogo

INSERT INTO adm.Catalogo VALUES 
(NULL, 'CONYUGUE', 'CONYUGUE', @CatalogoPadre, 1,1,0),
(NULL, 'HIJO/A', 'HIJO/A', @CatalogoPadre, 1,1,0)

ALTER TABLE DetalleCargasFamiliares ADD Parentesco INT

GO
/*****************  Tarea 25  ************************
	Proyecto: RRHH									*
	Fecha: 19/feb/2021
	Descripción: Generar un check junto al campo Año 
	de Finalización que permita escoger entre las 
	opciones de Finalizado y En Curso, este chek 
	debe habilitarse siempre y cuando se escoja el 
	año actual
*****************************************************/
ALTER TABLE DetalleEstudios ADD FINALIZADO BIT
GO
DECLARE @AñoActual INT = YEAR(GETDATE())
UPDATE DetalleEstudios SET Finalizado = 1 WHERE AnioFinalizacion < @AñoActual
UPDATE DetalleEstudios SET Finalizado = 0 WHERE AnioFinalizacion = @AñoActual
GO
/*****************  Tarea 33  ************************
	Proyecto: RRHH									*
	Fecha: 22/feb/2021
	Descripción: Generar un nuevo botón que permita 
	únicamente visualizar la ficha de ingreso, esto 
	es para el caso del usuario de la Doctora que 
	ella necesita ver los datos del nuevo empleado.
*****************************************************/
DECLARE @Rol VARCHAR(20) = 'MEDICO'
DECLARE @Perfil VARCHAR(20) = 'VISUALIZADOR FICHA INGRESO'
DECLARE @RolId INT
DECLARE @PerfilId INT
DECLARE @MenuId INT

INSERT INTO adm.Rol VALUES(@Rol, 'ROL QUE PERMITE VISUALIZACION DE USUARIOS',1)
INSERT INTO adm.Perfil VALUES(@Perfil,'VISUALIZACION FICHA INGRESO',1)

SELECT @RolId = IdRol FROM adm.Rol WHERE Nombre = @Rol
SELECT @PerfilId = IdPerfil FROM adm.Perfil WHERE NombrePerfil = @Perfil

INSERT INTO  adm.RolPerfil VALUES (@RolId, @PerfilId)

SELECT @MenuId = IdMenu FROM adm.Menu WHERE NombreMenu = 'Ficha de Ingreso'

INSERT INTO adm.PerfilMenu VALUES (@PerfilId, @MenuId)

DECLARE @PadreAccionSistema VARCHAR(50) = 'ACCIONES-SIST-01'
DECLARE @VisualizarAccionSistema VARCHAR(50)= 'ACCIONES-SIST-VISUALIZAR-REGISTRO'
DECLARE @IdPadre INT
DECLARE @IdVisualizarAccion INT
SELECT @IdPadre = IdCatalogo FROM adm.Catalogo WHERE CodigoCatalogo = @PadreAccionSistema

INSERT INTO adm.Catalogo VALUES(@VisualizarAccionSistema,'VISUALIZAR REGISTRO','_IndexGrid',@IdPadre,1,1,0)

SELECT @IdVisualizarAccion = IdCatalogo FROM adm.Catalogo WHERE CodigoCatalogo = @VisualizarAccionSistema 

INSERT INTO adm.RolMenuPermiso VALUES (@RolId, @PerfilId, @MenuId,@IdVisualizarAccion,1,1,GETDATE(), GETDATE(),1)