#region

using System.Data.Entity.Migrations;
using System.Linq;
using RIS.Model;

#endregion

namespace RIS.Data.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<DatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(DatabaseContext context)
        {
            context.FilterFields.AddOrUpdate(field => field.Id, new FilterField
            {
                Id = 1,
                Name = "Alle"
            }, new FilterField
            {
                Id = 2,
                Name = "Einsatzmittel"
            }, new FilterField
            {
                Id = 3,
                Name = "Bemerkung"
            }, new FilterField
            {
                Id = 4,
                Name = "Ort"
            }, new FilterField
            {
                Id = 5,
                Name = "Straße"
            }, new FilterField
            {
                Id = 6,
                Name = "Hausnummer"
            }, new FilterField
            {
                Id = 7,
                Name = "Objekt"
            }, new FilterField
            {
                Id = 8,
                Name = "Abschnitt"
            }, new FilterField
            {
                Id = 9,
                Name = "Kreuzung"
            }, new FilterField
            {
                Id = 10,
                Name = "Station"
            }, new FilterField
            {
                Id = 11,
                Name = "Schlagwort"
            }, new FilterField
            {
                Id = 12,
                Name = "Stichwort1"
            }, new FilterField
            {
                Id = 13,
                Name = "Stichwort2"
            }, new FilterField
            {
                Id = 14,
                Name = "Stichwort3"
            });

            context.FileprintConditions.AddOrUpdate(condition => condition.Id, new FileprintCondition
            {
                Id = 1,
                Name = "Ort ist"
            }, new FileprintCondition
            {
                Id = 2,
                Name = "Straße ist"
            }, new FileprintCondition
            {
                Id = 3,
                Name = "Hausnummer ist"
            }, new FileprintCondition
            {
                Id = 4,
                Name = "Straße + Hausnummer ist"
            }, new FileprintCondition
            {
                Id = 5,
                Name = "Objekt ist"
            }, new FileprintCondition
            {
                Id = 6,
                Name = "Ort + Straße ist"
            });

            context.AaoConditions.AddOrUpdate(condition => condition.Id, new AaoCondition
            {
                Id = 1,
                Name = "Schlagwort enthält"
            }, new AaoCondition
            {
                Id = 2,
                Name = "Stichwort enthält"
            }, new AaoCondition
            {
                Id = 3,
                Name = "Alarmiertes Fahrzeug"
            }, new AaoCondition
            {
                Id = 4,
                Name = "Einsatzort ist"
            }, new AaoCondition
            {
                Id = 5,
                Name = "Einsatzort ist nicht"
            });

            /* SAMPLE DATA */
            if (context.Vehicles.Count() == 0 && context.Pagers.Count() == 0)
            {
                context.Vehicles.Add(new Vehicle
                {
                    Name = "Musterhausen 21/1",
                    BosIdentifier = "6D884211",
                    ViewText = "21/1",
                    MainRow = 0,
                    MainColumn = 0,
                    FaxText = "Musterhausen 21/1"
                });
                context.Vehicles.Add(new Vehicle
                {
                    Name = "Musterhausen 30/1",
                    BosIdentifier = "6D884301",
                    ViewText = "30/1",
                    MainRow = 1,
                    MainColumn = 0,
                    FaxText = "Musterhausen 30/1"
                });
                context.Vehicles.Add(new Vehicle
                {
                    Name = "Musterstadt 40/1",
                    BosIdentifier = "6703665",
                    ViewText = "40/1",
                    MainRow = 2,
                    MainColumn = 0,
                    FaxText = "Musterstadt 40/1"
                });
                context.Vehicles.Add(new Vehicle
                {
                    Name = "Musterstadt 53/1",
                    BosIdentifier = "6703667",
                    ViewText = "53/1",
                    MainRow = 3,
                    MainColumn = 0,
                    FaxText = "Musterstadt 53/1"
                });
                context.Vehicles.Add(new Vehicle
                {
                    Name = "Musterhausen 10/1",
                    BosIdentifier = "6D884101",
                    ViewText = "10/1",
                    MainRow = 0,
                    MainColumn = 1,
                    FaxText = "Musterhausen 10/1"
                });
                context.Vehicles.Add(new Vehicle
                {
                    Name = "Musterstadt 88/1",
                    BosIdentifier = "6703677",
                    ViewText = "88/1",
                    MainRow = 3,
                    MainColumn = 1,
                    FaxText = "Musterstadt 88/1"
                });

                context.Pagers.Add(new Pager
                {
                    Identifier = "00118",
                    Name = "ZVEI: Musterhausen",
                    Priority = true
                });
                context.Pagers.Add(new Pager
                {
                    Identifier = "00119",
                    Name = "ZVEI: Musterhausen (Bereitschaft)",
                    Priority = false
                });
                context.Pagers.Add(new Pager
                {
                    Identifier = "00818",
                    Name = "ZVEI: Musterhausen (Mannschaft)",
                    Priority = false
                });
                context.Pagers.Add(new Pager
                {
                    Identifier = "12345672",
                    Name = "POCSAG: Musterstadt",
                    Priority = false
                });
                context.Pagers.Add(new Pager
                {
                    Identifier = "01121121",
                    Name = "POCSAG: Musterstadt (Bereitschaft)",
                    Priority = false
                });
                context.Pagers.Add(new Pager
                {
                    Identifier = "01121122",
                    Name = "POCSAG: Musterstadt (Mannschaft)",
                    Priority = true
                });
            }

            context.SaveChanges();
        }
    }
}