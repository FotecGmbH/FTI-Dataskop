# Datenbank-Umstellung SQL-Server <-> Postgres

## Umstellung auf Postgres-DB:
	- In Db.cs: boolean "UsePostgres" auf true stellen
	- In WebSettings.cs (oder über bs.xml): Datenbank-Properties (aus IAppSettingsDataBase) richtig stellen

## Migration:
	- mit aktuellen SQLServer-Einstellungen migrieren (Add-Migration und Update-Database) - unverändert
	- Migration Postgres-DB: siehe oben ("Umstellungen auf Postgres-DB") UND "Default-Project" in Package Manager Console auf "Database.Postgres.Migrations" (statt "Database" -> andernfalls erscheint Fehlermeldung bei Add-Migration-Befehl)