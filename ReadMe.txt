Contact Manager – Semesterprojekt Programming Foundation II

Technik:
- C# / .NET 8 Windows Forms
- Entity Framework Core 8
- SQLite
- Dependency Injection
- NLog

Projektstruktur:
- Models: Person, Employee, Trainee, Customer, CustomerContactNote
- Services: EmployeeService, CustomerService
- Infrastructure: ApplicationDbContext + EF-Core-Migrationen
- UI: FrmMain, FrmSaveEmployee, FrmSaveCustomer, FrmCustomerNotes

Start in Visual Studio:
1. src/ContactManager.sln öffnen.
2. NuGet-Pakete wiederherstellen lassen.
3. ContactManager als Startprojekt wählen.
4. Build -> Projektmappe erstellen.
5. Mit F5 starten.

Beim Start werden ausstehende EF-Core-Migrationen automatisch angewendet.
Die SQLite-Datei liegt im Data-Unterordner des Ausgabeverzeichnisses.

Wichtige Hinweise:
- Mitarbeitende erhalten beim Erstellen automatisch eine GUID-Mitarbeiternummer.
- Aktiv/Passiv ist ein Status und löscht keinen Datensatz.
- Kundennotizen werden mit Datum und Uhrzeit dauerhaft gespeichert.
- Die alte Contacts-Tabelle bleibt als Sicherheitskopie im Datenbankschema erhalten.

