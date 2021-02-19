USE RRHH
GO
/*****************  Tarea 1  ************************
	Proyecto: RRHH									*
	Fecha: 09/feb/2021
	Descripci�n: Empresa: Retirar del listado Finzo, 
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
	Descripci�n: "Estado Civil: Aumentar en el cat�logo 
	""Uni�n de Hecho"""
*****************************************************/
DECLARE @CatalogoPadre INT
SELECT @CatalogoPadre = IdCatalogo FROM adm.Catalogo WHERE CodigoCatalogo = 'ESTADO-CIVIL-01'
INSERT INTO adm.Catalogo (CodigoCatalogo, NombreCatalogo, DescripcionCatalogo, IdCatalogoPadre, EstadoCatalogo,IdEmpresa,Eliminado)
VALUES (NULL,'UNI�N DE HECHO','UNI�N DE HECHO', @CatalogoPadre, 1, 1, 0)
GO
/*****************  Tarea 18  ************************
	Proyecto: RRHH									*
	Fecha: 19/feb/2021
	Descripci�n: Referencia: Crear este nuevo campo 
		en Direcci�n (obligatorio)
*****************************************************/
ALTER TABLE FichaIngreso ADD Referencia VARCHAR(100)
GO

/*****************  Tarea 19  ************************
	Proyecto: RRHH									*
	Fecha: 19/feb/2021
	Descripci�n: Sector: Crear este nuevo campo 
		en Direcci�n (obligatorio)
*****************************************************/
ALTER TABLE FichaIngreso ADD Sector VARCHAR(100)
GO
/*****************  Tarea 25  ************************
	Proyecto: RRHH									*
	Fecha: 19/feb/2021
	Descripci�n: Tipo de Estudio: El Cat�logo debe
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
	Descripci�n: parentesco: Agregar nuevo campo 
	con una lista desplegable que contenga a 
	C�nyuge e Hijos
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
	Descripci�n: Generar un check junto al campo A�o 
	de Finalizaci�n que permita escoger entre las 
	opciones de Finalizado y En Curso, este chek 
	debe habilitarse siempre y cuando se escoja el 
	a�o actual
*****************************************************/


