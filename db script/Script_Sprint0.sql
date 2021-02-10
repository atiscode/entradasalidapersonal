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
INSERT INTO adm.Catalogo (CodigoCatalogo, NombreCatalogo, DescripcionCatalogo, IdCatalogoPadre, EstadoCatalogo,IdEmpresa,Eliminado)
VALUES (NULL,'UNI�N DE HECHO','UNI�N DE HECHO', 123, 1, 1, 0)
GO