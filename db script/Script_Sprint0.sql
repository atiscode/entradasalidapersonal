/****************************************************
	Proyecto: RRHH									*
	Fecha: 09/feb/2021
	Descripción: Empresa: Retirar del listado Finzo, 
		Evaluar , cambiar APS PTV por  Kickoff / outside
		box por OPINO
*****************************************************/

DELETE FROM adm.Catalogo WHERE NombreCatalogo IN ('FINZO')
UPDATE adm.Catalogo SET NombreCatalogo = 'KICKOFF', CodigoCatalogo = 'KICKOFF' WHERE CodigoCatalogo = 'PTV'
UPDATE adm.Catalogo SET NombreCatalogo = 'OPINO', CodigoCatalogo = 'OPINO' WHERE CodigoCatalogo = 'OSTB'