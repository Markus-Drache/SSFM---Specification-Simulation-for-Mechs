using Microsoft.Data.Sqlite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;


// needed time 5h 25.09.2025
// needed time 10h 07.10.2025
// needed time 5h 08.10.2025
// needed time 6h 09.10.2025
// needed time 4h 20.10.2025
// needed time 6h 21.10.2025
// needed time 7h 22.10.2025
// needed time 7h 23.10.2025
// needed time 5h 24.10.2025
// needed time 2h 03.11.2025
// needed time 4h 06.11.2025
// needed time 5h 08.11.2025
// needed time 4h 19.11.2025
// needed time 7h 23.11.2025
// needed time 1h 26.11.2025
// needed time 7h 29.11.2025
// needed time 4h 30.11.2025

namespace DB_Conektion
{
    class Conect
    {
        // Database Connection Values
        // private string server = "localhost";
        private string database = "DATABASE.db";
        private string db_connection_string = "";
        private string mainfolder = "";
        private bool connected = false;

        // Class Values
        private int year = 0;
        private string fraction = "";
        private int fractionID = 0;
        private string technologyniveau = "";
        private string technologyavailability = "";

        // Ersatz Konstanten
        private List<Help_list> FractionList;

        // Konstruktor

        public Conect(string MF)
        {
            // Standard-Konstruktor
            // Stellt die Verbindung zur Datenbank her
            mainfolder = MF; // Initialisierung
                             // mainfolder = AppContext.BaseDirectory; // .NET 6/7/8
                             // .NET Framework Alternative: AppDomain.CurrentDomain.BaseDirectory
            db_connection_string = GetConnectionString();


            // z.B. in Program.Main(), App.xaml.cs (WPF) oder vor dem ersten DB-Zugriff:
            Batteries_V2.Init();

            CreateFractionList();

        }

        // Absoluter Pfad zur DB: <Ausgabeverzeichnis>\DATABASE.db
        /// <summary>
        /// Constructs and returns the connection string for accessing the SQLite database.
        /// </summary>
        /// <remarks>
        /// The method searches for the database file in the specified main folder and its
        /// subdirectories. The resulting connection string is configured for read-only access and shared cache
        /// mode.
        /// </remarks>
        /// <returns>
        /// A connection string that can be used to connect to the SQLite database.
        /// </returns>
        private string GetConnectionString()
        {
            // Sucht die Datei im Hauptverzeichnis und allen Unterverzeichnissen
            string? dbPath = FindeDatei(FindAncestorWithFile(AppContext.BaseDirectory, mainfolder), database);
            // string dbPath = "G:\\BT_Projekt_Abendschule\\DB_Conektion\\DB_Conektion\\DATABASE.db";

            if (dbPath == null)
            {
                // throw new FileNotFoundException($"Datenbankdatei '{database}' wurde im Verzeichnis '{mainfolder}' und seinen Unterverzeichnissen nicht gefunden.");
                connected = false;
            }
            else
            {
                connected = true;
            }

            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadOnly, // Nur-Lesezugriff
                Cache = SqliteCacheMode.Shared
            };
            return builder.ToString();
        }

        /// <summary>
        /// Aufruf einer SQL-Abfrage, die eine Ergebnismenge zurückgibt, als DataTable.
        /// </summary>
        private DataTable QueryAsDataTable(string sql) // sql = "SELECT * FROM ..."
        {
            if (!connected)
            {
                return new DataTable(); // Leere DataTable zurückgeben, wenn keine Verbindung besteht
            }
            using var con = new SqliteConnection(db_connection_string);
            using var cmd = new SqliteCommand(sql, con);
            con.Open();
            using var da = cmd.ExecuteReader();
            var table = new DataTable();
            table.Load(da); // Füllt die DataTable mit den Ergebnissen
            return table;
        }


        // Methoden

        // Verbindung erneut herstellen
        public void Reconnect(string MF)
        {
            mainfolder = MF; // Initialisierung

            db_connection_string = GetConnectionString();
        }
        public bool IsConnected()
        {
            return connected;
        }

        // Fraction
        public void SetFraction(string fraction)
        {
            if (fraction == "")
            {
                throw new ArgumentException("Fraction cannot be empty.");
            }
            DataTable validFractions = GetFractions();
            if (validFractions.AsEnumerable().Any(row => row.Field<string>("fraction") == fraction))
            {
                throw new ArgumentException("Invalid fraction.");
            }
            this.fraction = fraction;
            DataTable ID = QueryAsDataTable($"SELECT id from fraction where Name = '{fraction}';");
            fractionID = ID.Rows[0].Field<int>(0);
            CreateFractionList();
        }
        public string GetFraction()
        {
            return fraction;
        }
        public List<Help_list> GetAllFraction(bool setfrac = false)
        {
            if (setfrac)
            {
                CreateFractionList();
            }
            return FractionList;
        }
        /// <summary>
        /// Gets all factions based on year as from the database as a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="sort">The column to sort the results by. Default is "Emerged".</param>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// Fraction (string)
        /// </returns>
        public DataTable GetFractions(string sort = "Emerged")
        {
            string sql = "SELECT name as fraktion " +
                      "from fraction ";

            if (year != 0)
            {
                sql += $"where Emerged <= {year} " +
                  $"and(Disappeared is null or {year} <= Disappeared) ";
            }

            sql += $"order by {sort}; ";

            return QueryAsDataTable(sql);
        }
        /// <summary>
        /// Retrieves an array of integers representing the available fractions based on the database query and the
        /// current state of the <c>FractionList</c>.
        /// </summary>
        /// <remarks>This method queries the database to retrieve a fraction ID and then processes the
        /// <c>FractionList</c> to determine the related fractions. The resulting array includes all fractions that are
        /// associated with the queried fraction ID.</remarks>
        /// <returns>An array of integers representing the available fractions. Returns an empty array if no fractions are found
        /// in the database.</returns>
        private long[] FractionAveleble()
        {
            string sql = $"SELECT id from (" +
                $"select * from fraction " +
                $"where Emerged <= {year} and (Disappeared is null or {year} <= Disappeared)" +
                $") where Name = {this.fraction}";
            DataTable dt = QueryAsDataTable(sql);
            long? frac = (long)dt.Rows[0].Field<long?>(0);
            if (frac == null)
            {
                return new long[0];
            }

            long[] result = new long[] { 111, 112 };

            // FractionList
            for (int i = 0; i < FractionList.Count; i++)
            {
                for (int j = 0; j < FractionList[i].ids.Length; j++)
                {
                    if (FractionList[i].ids[j] == frac)
                    {
                        for (int k = 0; k <= j; k++)
                        {
                            // for (int l = 0; l < result.Length; l++)
                            // if (result[l] != FractionList[i].ids[k])
                            if (!result.Contains(FractionList[i].ids[k]))
                            {
                                Array.Resize(ref result, result.Length + 1);
                                result[result.Length - 1] = FractionList[i].ids[k];
                            }
                        }

                        break;
                    }
                }
            }

            return result;
        }


        // Year
        /// <summary>
        /// Sets the year to the specified value.
        /// </summary>
        /// <param name="year">The year to set. Must be between 1900 and 3150, inclusive, or 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="year"/> is not within the range 1900 to 3150, inclusive, and is not 0.</exception>
        public void SetYear(int year)
        {
            if (year != 0 || year >= 1900 && year <= 3150)
            {
                throw new ArgumentOutOfRangeException("Year must be between 1900 and 3150 or 0.");
            }
            this.year = year;
        }
        public int GetYear()
        {
            return year;
        }

        // technologyniveau
        public void Settechnologyniveau(string technologyniveau)
        {
            if (technologyniveau == "")
            {
                throw new ArgumentException("technologyniveau cannot be empty.");
            }
            DataTable validNiveaus = GetTechnologyNiveaus();

            if (validNiveaus.AsEnumerable().Any(row => row.Field<string>("Name") == technologyniveau))
            {
                throw new ArgumentException("Invalid technologyniveau.");
            }

            this.technologyniveau = technologyniveau;
        }
        public string Gettechnologyniveau()
        {
            return technologyniveau;
        }
        /// <summary>
        /// Gets all available technology niveaus from the database as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// technologyniveau (string)
        /// </returns>
        public DataTable GetTechnologyNiveaus()
        {
            string sql = "SELECT * FROM technology_level;";
            return QueryAsDataTable(sql);
        }

        // Technologievverfügbarkeit
        public void SetTechnologyAvailability(string technologyavailability)
        {
            if (technologyavailability == "")
            {
                throw new ArgumentException("technologyavailability cannot be empty.");
            }
            DataTable validAvailabilitys = GetTechnologyAvailabilityen();
            if (validAvailabilitys.AsEnumerable().Any(row => row.Field<string>("Name") == technologyavailability))
            {
                throw new ArgumentException("Invalid technologyavailability.");
            }
            this.technologyavailability = technologyavailability;
        }
        public string GetTechnologyAvailability()
        {
            return technologyavailability;
        }
        /// <summary>
        /// Gets all technology from the database as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="DataTable"/> contains the columns:
        /// technologyverfügbarkeit (string)
        /// </returns>
        public DataTable GetTechnologyAvailabilityen()
        {
            string sql = $"SELECT * FROM rule_level;";
            return QueryAsDataTable(sql);
        }


        // InternealeStruktur
        /// <summary>
        /// Gets all available internal structures based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), weight (int), needed_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableInternealStruktur()
        {
            string sql = "select * from systems where ItemTypeID = 28;";
            return QueryAsDataTable(sql);
        }

        // Slots
        /// <summary>
        /// Gets the available slots based on tonnage, technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// torso (int), arm (int), leg (int)
        /// </returns>
        public DataTable GetAvailableSlots(int tonnage = 0)
        {
            string sql = $"SELECT Center_Torso, Left_Right_Torso, Arm, Leg FROM bm_slot where Tonage = {tonnage};";
            return QueryAsDataTable(sql);
        }

        // Internal Structure Integrity
        /// <summary>
        /// Gets the internal structure integrity based on tonnage as a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="tonnage"></param>
        /// <returns>
        /// center_torso (int), left_right_torso (int), arm (int), leg (int)
        /// </returns>
        public DataTable GetAvailableInternalStructureIntegrity(int tonnage)
        {
            string sql = $"SELECT Center_Torso, Left_Right_Torso, Arm, Leg FROM structure_points where Tonage = {tonnage};";
            return QueryAsDataTable(sql);
        }

        // Musculature
        /// <summary>
        /// Gets all available musculature based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), weight (int), needed_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableMusculature()
        {
            string sql = "select * from systems where ItemTypeID = 27;";
            return QueryAsDataTable(sql);
        }

        // Activator
        /// <summary>
        /// Gets all available activators based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), weight (int), needed_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableActivator()
        {
            string sql = "SELECT * FROM *;";
            return QueryAsDataTable(sql);
        }

        // Gyroscope
        /// <summary>
        /// Gets all available gyroscopes based on factions, year, tonage , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// the <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), weight (int), needed_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableGyroscope()
        {
            string sql = "select * from systems where ItemTypeID = 21;";
            return QueryAsDataTable(sql);
        }

        // Reactor
        /// <summary>
        /// Gets all available reactors based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), needed_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableReactor()
        {
            string sql = "select * from systems where ItemTypeID = 19;";
            return QueryAsDataTable(sql);
        }

        // Reactor weight
        /// <summary>
        /// Gets the specific reactor weight based on reactorwert and reacktortype as a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="reacktorwert"></param>
        /// <param name="reacktortype"></param>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// weight (int)
        /// </returns>
        public DataTable GetAvailableReactorWeight(int reacktorwert, string reacktortype)
        {
            string sql = $"select {reacktortype} from reactor_weight where RW = {reacktorwert};";
            return QueryAsDataTable(sql);
        }

        // Special Movement
        /// <summary>
        /// Gets all available special movment based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), weight (int), needed_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableSpecialMovement()
        {
            string sql = "select * from systems where ItemTypeID = 18;";
            return QueryAsDataTable(sql);
        }

        // Head Size
        /// <summary>
        /// Gets all available head sizes based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), weight (int), needed_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableHeadSize()
        {
            string sql = $"SELECT * FROM *;";
            return QueryAsDataTable(sql);
        }

        // Cockpit System
        /// <summary>
        /// Gets all available cockpit systems based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), weight (int), available_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableCockpitSystem()
        {
            string sql = "select * from systems where ItemTypeID = 29;";
            return QueryAsDataTable(sql);
        }

        // Sensors
        /// <summary>
        /// Gets all available sensors based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), weight (int), needed_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableSensors()
        {
            string sql = $"SELECT * FROM *;";
            return QueryAsDataTable(sql);
        }

        // Armor
        /// <summary>
        /// Gets all available armor based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), weight (int), needed_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableArmor()
        {
            string sql = "select * from systems where ItemTypeID = 8;";
            return QueryAsDataTable(sql);
        }

        // Heat Sink
        /// <summary>
        /// Gets all available heat sinks based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// type (string), cost (int), weight (int), needed_slotz (int), specialRules (string)
        /// </returns>
        public DataTable GetAvailableHeatSink()
        {
            string sql = "select * from systems where ItemTypeID = 22;";
            return QueryAsDataTable(sql);
        }

        // Equipment
        /// <summary>
        /// Gets all available equipment based on factions, year , technology niveaus and technology Availability as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> contains the columns:
        /// ID (string), type (string), cost (int), weight (int), needed_slotz (int), range (string), damage (string), heat (int),
        /// ammo_per_ton (string), ammo_cost (int), special_ammo_available (string), attack_value_ammo (int), defense_value_ammo (int),
        /// aim_assist_computer (string), hit_probability_modifier (int), aim_assist_computer_value (int),
        /// attack_value (int), defense_value (int), attack_type (string), specialRules (string)
        /// </returns>
        public DataTable GetAvailableEquipment()
        {
            long[] availableFractions = FractionAveleble();
            string sql;
            // sql = $"SELECT * FROM *;";

            sql = "select i.name || ' ' || Specification as Name, " +
                    "(SELECT GROUP_CONCAT(name, ', ') AS namen_DAMAGE_TYPES " +
                    "FROM(SELECT dt.shortname as name " +
                    "FROM   item_damage_type idt " +
                    "JOIN   damage_type dt ON dt.ID = idt.damage_type_id " +
                    "WHERE  idt.item_id = i.ID-- Waffen ID " +
                    "order by dt.name " +
                    ")) as damage_types, " +
                    "Heat, Damage, " +
                    "Range_Min || '/' || Range_Short || '/' || Range_Medium || '/' || Range_Long as Range, " +
                    "Accuracy, " +
                    "case " +
                    "when Aim_Control_Computer is 0 then 'No' " +
                    "when Aim_Control_Computer is 1 then 'Yes' " +
                    "else 'N/A' " +
                    "end as Aim_Control_Computer, " +
                    "Tonage, BattleValue, " +
                    "Price, " +
                    "Slot, technology_level.ShortName as Tech_Level " +
                    "from items i " +
                    "JOIN technology_level on i.tech_level = technology_level.ID " +
                    "where i.tech_level <= (select ID from technology_level where Name = \"" + technologyniveau + "\") " +
                    "and i.ID in (select item_id from items_rule_level where rule_level <= (select ID from rule_level where Name = \"" + technologyavailability + "\"))"
                    ;

            if (fraction != "ANY" || year != 0)
            {
                sql += "and i.ID in (select item_id from items_hist where ";
                if (year != 0)
                {
                    sql += "(obsolete is null or obsolete >= " + year + " and (lost is null or " + year + " <= lost)) ";
                }
                if (fraction != "ANY" || year != 0)
                {
                    sql += "and ";
                }
                if (fraction != "ANY")
                {
                    sql += "fraktion = (select id from fraction where name = \"" + string.Join(",", availableFractions) + "\") ";
                }
            }
            sql += "order by i.ID;";

            return QueryAsDataTable(sql);
        }

        // single Equipment by ID
        /// <summary>
        /// Gets a single equipment by its ID as a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// returns all data about the equipment
        /// </returns>
        public DataTable GetSingleEquipmentByID(int id)
        {
            string sql = $"SELECT * FROM items WHERE id = {id};";
            return QueryAsDataTable(sql);
        }


        // Fractions List erstellen
        /// <summary>
        /// Initializes and populates the <c>FractionList</c> with predefined data or dynamically generated data based
        /// on specific conditions.
        /// </summary>
        /// <remarks>This method clears the existing <c>FractionList</c> and repopulates it. If the
        /// predefined condition is met,  the list is populated with a hardcoded set of <c>Help_list</c> objects.
        /// Otherwise, the list is dynamically  generated using data retrieved from a database.</remarks>
        private void CreateFractionList()
        {
            if (FractionList != null)
            {
                FractionList.Clear();
            }
            else
            {
                FractionList = new List<Help_list>();
            }

            if (false)
            {
                FractionList = new List<Help_list>
                {
            new Help_list(new long[] {1, 2,  3,  4,  54}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  4,  55, 69}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  55, 69, 109}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  56}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  56, 107}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  57}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  57, 108}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  58}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  4,  59}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  59, 78, 103}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  60}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  60, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  61}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  61, 96}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  61, 96, 110}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  62, 60}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  62, 60, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  63}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  64, 67}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  4,  65}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  4,  66, 74, 114}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  67}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  4,  68, 99}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  69}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  69, 109}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  70}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  4,  71, 72}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  71, 72, 79}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  71, 72, 106}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  72}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  72, 79}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  72, 106}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  4,  73}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  54}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  55, 69}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  55, 69, 109}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  56}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  56, 107}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  57}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  57, 108}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  58}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  59}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  59, 78, 103}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  60}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  60, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  61}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  61, 96}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  61, 96, 110}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  62, 60}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  62, 60, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  63}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  64, 67}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  65}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  66, 74, 114}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  67}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  68, 99}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  69}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  69, 109}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  70}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  71, 72}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  71, 72, 79}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  71, 72, 106}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  72}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  72, 79}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  72, 106}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  4,  73}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  34, 41}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  35}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  36}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  37, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  38, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  40}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  40, 77, 80, 93}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  42}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  43}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  43, 102}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  45, 44}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  46, 44}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  48, 43}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  48, 43, 102}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  49}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  94}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  33, 3,  95}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  34, 41}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  35}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  36}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  37, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  38, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  40}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  40, 77, 80, 93}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  42}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  43}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  43, 102}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  45, 44}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  46, 44}, "TOT"),
            new Help_list(new long[] {1, 2,  3,  48, 43}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  48, 43, 102}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  49}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  94}, "EXISTS"),
            new Help_list(new long[] {1, 2,  3,  95}, "EXISTS"),
            new Help_list(new long[] {1, 2,  14, 12}, "EXISTS"),
            new Help_list(new long[] {1, 2,  14, 12, 53, 103}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  54}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  55, 69}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  55, 69, 109}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  56}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  56, 107}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  57}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  57, 108}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  58}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  59}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  59, 78, 103}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  60}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  60, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  61}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  61, 96}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  61, 96, 110}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  62, 60}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  62, 60, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  63}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  64, 67}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  65}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  66, 74, 114}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  67}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  68, 99}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  69}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  69, 109}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  70}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  71, 72}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  71, 72, 79}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  71, 72, 106}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  72}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  72, 79}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  72, 106}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  4,  73}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  54}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  55, 69}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  55, 69, 109}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  56}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  56, 107}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  57}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  57, 108}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  58}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  59}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  59, 78, 103}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  60}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  60, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  61}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  61, 96}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  61, 96, 110}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  62, 60}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  62, 60, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  63}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  64, 67}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  65}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  66, 74, 114}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  67}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  68, 99}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  69}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  69, 109}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  70}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  71, 72}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  71, 72, 79}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  71, 72, 106}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  72}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  72, 79}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  72, 106}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  4,  73}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  34, 41}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  35}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  36}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  37, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  38, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  40}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  40, 77, 80, 93}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  42}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  43}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  43, 102}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  45, 44}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  46, 44}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  48, 43}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  48, 43, 102}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  49}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  94}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  33, 3,  95}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  34, 41}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  35}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  36}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  37, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  38, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  40}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  40, 77, 80, 93}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  42}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  43}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  43, 102}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  45, 44}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  46, 44}, "TOT"),
            new Help_list(new long[] {1, 2,  29, 3,  48, 43}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  48, 43, 102}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  49}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  94}, "EXISTS"),
            new Help_list(new long[] {1, 2,  29, 3,  95}, "EXISTS"),
            new Help_list(new long[] {1, 2,  30}, "EXISTS"),
            new Help_list(new long[] {1, 2,  30, 81}, "EXISTS"),
            new Help_list(new long[] {1, 2,  31}, "EXISTS"),
            new Help_list(new long[] {1, 2,  31, 82}, "EXISTS"),
            new Help_list(new long[] {1, 2,  32, 99}, "EXISTS"),
            new Help_list(new long[] {1, 2,  37, 97}, "EXISTS"),
            new Help_list(new long[] {1, 2,  47, 43}, "EXISTS"),
            new Help_list(new long[] {1, 2,  47, 43, 102}, "EXISTS"),
            new Help_list(new long[] {1, 6,  5, }, "EXISTS"),
            new Help_list(new long[] {1, 6,  5,  50, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 6,  5,  50, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 6,  5,  50, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 6,  5,  50, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 6,  5,  51, 5, }, "EXISTS"),
            new Help_list(new long[] {1, 7,  5, }, "EXISTS"),
            new Help_list(new long[] {1, 7,  5,  50, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 7,  5,  50, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 7,  5,  50, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 7,  5,  50, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 7,  5,  51, 5, }, "EXISTS"),
            new Help_list(new long[] {1, 8,  5, }, "EXISTS"),
            new Help_list(new long[] {1, 8,  5,  50, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 8,  5,  50, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 8,  5,  50, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 8,  5,  50, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 8,  5,  51, 5, }, "EXISTS"),
            new Help_list(new long[] {1, 9,  5, }, "EXISTS"),
            new Help_list(new long[] {1, 9,  5,  50, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 9,  5,  50, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 9,  5,  50, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 9,  5,  50, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 9,  5,  51, 5, }, "EXISTS"),
            new Help_list(new long[] {1, 10, 5, }, "EXISTS"),
            new Help_list(new long[] {1, 10, 5,  50, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 10, 5,  50, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 10, 5,  50, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 10, 5,  50, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 10, 5,  51, 5, }, "EXISTS"),
            new Help_list(new long[] {1, 11, 5, }, "EXISTS"),
            new Help_list(new long[] {1, 11, 5,  50, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 11, 5,  50, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 11, 5,  50, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 11, 5,  50, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 11, 5,  51, 5, }, "EXISTS"),
            new Help_list(new long[] {1, 13, 12, 53, 103}, "EXISTS"),
            new Help_list(new long[] {1, 13, 12}, "EXISTS"),
            new Help_list(new long[] {1, 15, 12, 53, 103}, "EXISTS"),
            new Help_list(new long[] {1, 15, 12}, "EXISTS"),
            new Help_list(new long[] {1, 17, 16, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 17, 16, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 17, 16, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 17, 16, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 18, 16, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 18, 16, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 18, 16, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 18, 16, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 19, 16, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 19, 16, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 19, 16, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 19, 16, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 21, 20, 52, 31}, "EXISTS"),
            new Help_list(new long[] {1, 21, 20, 86}, "EXISTS"),
            new Help_list(new long[] {1, 21, 20, 87}, "EXISTS"),
            new Help_list(new long[] {1, 21, 20, 87, 101, 104}, "EXISTS"),
            new Help_list(new long[] {1, 21, 20, 88, 104}, "EXISTS"),
            new Help_list(new long[] {1, 21, 20, 89, 105, 104}, "EXISTS"),
            new Help_list(new long[] {1, 21, 20, 90, 104}, "EXISTS"),
            new Help_list(new long[] {1, 21, 20, 91}, "EXISTS"),
            new Help_list(new long[] {1, 21, 20, 91, 100}, "EXISTS"),
            new Help_list(new long[] {1, 22, 20, 52, 31}, "EXISTS"),
            new Help_list(new long[] {1, 22, 20, 86}, "EXISTS"),
            new Help_list(new long[] {1, 22, 20, 87}, "EXISTS"),
            new Help_list(new long[] {1, 22, 20, 87, 101, 104}, "EXISTS"),
            new Help_list(new long[] {1, 22, 20, 88, 104}, "EXISTS"),
            new Help_list(new long[] {1, 22, 20, 89, 105, 104}, "EXISTS"),
            new Help_list(new long[] {1, 22, 20, 90, 104}, "EXISTS"),
            new Help_list(new long[] {1, 22, 20, 91}, "EXISTS"),
            new Help_list(new long[] {1, 22, 20, 91, 100}, "EXISTS"),
            new Help_list(new long[] {1, 23, 20, 52, 31}, "EXISTS"),
            new Help_list(new long[] {1, 23, 20, 86}, "EXISTS"),
            new Help_list(new long[] {1, 23, 20, 87}, "EXISTS"),
            new Help_list(new long[] {1, 23, 20, 87, 101, 104}, "EXISTS"),
            new Help_list(new long[] {1, 23, 20, 88, 104}, "EXISTS"),
            new Help_list(new long[] {1, 23, 20, 89, 105, 104}, "EXISTS"),
            new Help_list(new long[] {1, 23, 20, 90, 104}, "EXISTS"),
            new Help_list(new long[] {1, 23, 20, 91}, "EXISTS"),
            new Help_list(new long[] {1, 23, 20, 91, 100}, "EXISTS"),
            new Help_list(new long[] {1, 24, 20, 52, 31}, "EXISTS"),
            new Help_list(new long[] {1, 24, 20, 86}, "EXISTS"),
            new Help_list(new long[] {1, 24, 20, 87}, "EXISTS"),
            new Help_list(new long[] {1, 24, 20, 87, 101, 104}, "EXISTS"),
            new Help_list(new long[] {1, 24, 20, 88, 104}, "EXISTS"),
            new Help_list(new long[] {1, 24, 20, 89, 105, 104}, "EXISTS"),
            new Help_list(new long[] {1, 24, 20, 90, 104}, "EXISTS"),
            new Help_list(new long[] {1, 24, 20, 91}, "EXISTS"),
            new Help_list(new long[] {1, 24, 20, 91, 100}, "EXISTS"),
            new Help_list(new long[] {1, 26, 25, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 26, 25, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 26, 25, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 26, 25, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 27, 25, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 27, 25, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 27, 25, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 27, 25, 39, 76, 92}, "EXISTS"),
            new Help_list(new long[] {1, 28, 25, 39, 75, 84, 75}, "EXISTS"),
            new Help_list(new long[] {1, 28, 25, 39, 75}, "EXISTS"),
            new Help_list(new long[] {1, 28, 25, 39, 75, 85}, "EXISTS"),
            new Help_list(new long[] {1, 28, 25, 39, 76, 92}, "EXISTS")

                };
            }
            else
            {
                // FractionList = new List<Help_list> { };

                DataTable hist, frac;

                hist = QueryAsDataTable("select parent_id, child_id from fraction_tree;");
                frac = QueryAsDataTable("select ID, Emerged, Disappeared from fraction;");

                frac.PrimaryKey = new DataColumn[] { frac.Columns["ID"] };

                FillHelpList(hist, frac, 0);
            }
        }

        /// <summary>
        /// Füllt die Hilfsliste rekursiv basierend auf der Historie und den Fraktionsdaten.
        /// </summary>
        /// <param name="hist"></param>
        /// <param name="frac"></param>
        /// <param name="Year"></param>
        /// <param name="ID"></param>
        /// <param name="hlist"></param>
        private void FillHelpList(DataTable hist, DataTable frac, long Year, long ID = 1, long[] hlist = null)
        {
            hlist ??= new long[] { ID }; // wenn null → neue Instanz erstellen
            long[] hlist_t;
            long Year_t;

            foreach (DataRow row in hist.Rows)
            {
                bool found = false;

                if (ID == 33)
                {
                    bool debug = true; // Debugger-Breakpoint
                }

                if (row.Field<long>("parent_id") == ID
                    &&
                    !(ID == 75 && hlist[hlist.Length - 3] == 75)
                    )
                {
                    long? e, d;
                    e = frac.Rows.Find(row.Field<long>("child_id")).Field<long?>("Emerged");
                    d = frac.Rows.Find(row.Field<long>("child_id")).Field<long?>("Disappeared");

                    if (// false
                        frac.Rows.Find(row.Field<long>("child_id")).Field<long?>("ID") == 33
                        // Year == 2779
                        // && hlist.SequenceEqual(new long[] {1, 2, 29})
                        // && hlist.SequenceEqual(new long[] { 1, 2, 29, 3 })
                        // ||
                        // hlist.SequenceEqual(new long[] {1, 2, 29, 3, 33})
                        || hlist.SequenceEqual(new long[] { 1, 2, 29, 3, 33, 3 })
                        )
                    {
                        bool debug = true; // Debugger-Breakpoint
                    }

                    if (e >= Year
                        || (e <= Year
                        && (d == null || d > Year))
                        )
                    {
                        hlist_t = hlist.Append(row.Field<long>("child_id")).ToArray();

                        if (e <= Year && (d == null || d > Year))
                        {
                            long? pd = null;

                            if (d == null)
                            {
                                pd = frac.Rows.Find(row.Field<long>("parent_id")).Field<long?>("Disappeared");
                            }
                            else
                            {
                                if (d < frac.Rows.Find(row.Field<long>("parent_id")).Field<long?>("Disappeared"))
                                {
                                    pd = d;
                                }
                                else
                                {
                                    pd = ++Year;
                                }
                                //pd = d;
                            }

                            if (pd > Year)
                            {
                                Year_t = pd ?? Year;
                            }
                            else
                            {
                                Year_t = Year;
                            }
                        }
                        else
                        {
                            Year_t = e ?? Year;
                        }


                        DataTable kopie = hist.Copy();

                        long childId = row.Field<long>("child_id");
                        long parentId = row.Field<long>("parent_id");

                        // in der Kopie suchen:
                        DataRow[] rowsKopie = kopie.Select(
                            $"child_id = {childId} AND parent_id = {parentId}");

                        if (rowsKopie.Length > 0)
                        {
                            kopie.Rows.Remove(rowsKopie[0]);   // oder rowsKopie[0].Delete();
                        }

                        FillHelpList(kopie, frac, Year_t, row.Field<long>("child_id"), hlist_t);
                    }
                    found = true;
                }

                for (int i = 0; i < FractionList.Count; i++)
                {
                    if (FractionList[i].ids.SequenceEqual(hlist))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    if (frac.Rows.Find(ID).Field<long?>("Disappeared") == null
                        || frac.Rows.Find(ID).Field<long?>("Disappeared") > (year == 0 ? long.MaxValue : year)
                        )
                    {
                        FractionList.Add(new Help_list(hlist, "EXISTS"));
                    }

                    if (frac.Rows.Find(ID).Field<long?>("Disappeared") != null
                        && frac.Rows.Find(ID).Field<long?>("Disappeared") <= (year == 0 ? long.MaxValue : year)
                        && !hist.AsEnumerable().Any(r => r.Field<long>("parent_id") == ID)
                        )
                    {
                        FractionList.Add(new Help_list(hlist, "TOT"));
                    }
                }

            }

        }

        // Hilfsmethode: Sucht in den übergeordneten Verzeichnissen nach einer bestimmten Datei
        string? FindAncestorWithFile(string? startPath, string markerFileName)
        {
            if (startPath == null || !Directory.Exists(startPath))
            {
                return null;
            }
            var dir = new DirectoryInfo(startPath);
            while (dir != null)
            {
                string[] array = Directory.GetDirectories(dir.ToString());
                for (int i = 0; i < array.Length; i++)
                {
                    // string unterordner = array[i];
                    // string gefunden = FindeDatei(unterordner, );
                    // var test = Path.Combine(dir.FullName, markerFileName);
                    // if (unterordner == test) // Path.Combine(dir.FullName, markerFileName))
                    if (array[i].EndsWith(markerFileName))
                    {
                        return dir.FullName.ToString();
                    }
                }

                // if (File.Exists(Path.Combine(dir.FullName, markerFileName)))
                //     return dir.FullName.ToString();

                dir = dir.Parent;
            }
            return null;
        }

        // Rekursive Suche nach der Datei innerhalb eines Verzeichnisses
        string? FindeDatei(string? verzeichnis, string dateiname)
        {
            if (verzeichnis == null || !Directory.Exists(verzeichnis))
            {
                return null;
            }
            try
            {
                foreach (string datei in Directory.GetFiles(verzeichnis))
                {
                    if (Path.GetFileName(datei).Equals(dateiname, StringComparison.OrdinalIgnoreCase))
                    {
                        return datei;
                    }
                }

                foreach (string unterordner in Directory.GetDirectories(verzeichnis))
                {
                    string? gefunden = FindeDatei(unterordner, dateiname);
                    if (gefunden != null)
                    {
                        return gefunden;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Durchsuchen von '{verzeichnis}': {ex.Message}");
            }

            return null;
        }

    }

    public class Help_list
    {
        public long[] ids;
        public string status;

        public Help_list()
        {
            this.ids = new long[] { };
            this.status = "";
        }

        public Help_list(long[] ids, string status)
        {
            this.ids = ids;
            this.status = status;
        }

    }
}
