﻿using System.Text;
using CarApp.Helper;
using CarApp.Model;

namespace CarApp;

/// <summary>
/// Menu item record struct.
/// Where Name is the name of the menu item and Index is the index of the menu item.
/// Index is used to determine the selected menu item.
/// </summary>
internal record struct MenuItem(string Name, int Index)
{
    public static implicit operator (string, int)(MenuItem value)
    {
        return (value.Name, value.Index);
    }

    public static implicit operator MenuItem((string, int) value)
    {
        return new MenuItem(value.Item1, value.Item2);
    }
}

/// <summary>
/// Handles CarApp's main program logic.
/// </summary>
internal class Program
{
    static int _paganize = 10; ///< The pagination size for menus.
    static Car? _selectedCar; ///< The selected car object
    static Authentication _auth = new Authentication(); ///< Authentication object

    static readonly FuelPriceList _fuelPricelist = FuelPriceList.Instance; ///< Fuel price list object
    static readonly CarList _carList = CarList.Instance; ///< Car list object

    static int FirstAutomobileYear { get; } = 1886; ///< The year of the first car in the world

    static string? CurrentUser { get; set; }

    /// <summary>
    /// Login the user.
    /// Takes the username from the environment variables.
    /// </summary>
    /// <dependency>CarApp.Authentication</dependency>
    private static void Login()
    {
        string? username = Environment.GetEnvironmentVariable("USER_NAME");
        string msgDefault = "";
        Console.Clear();
        Header("Log ind");
        if (username != "")
            msgDefault = $"default \u001b[34m{username}\u001b[0m ";
        Console.Write($"Brugernavn {msgDefault}: ");
        string? username1 = Console.ReadLine();
        if (username1 != "")
            username = username1;

        Console.Write("Adgangskode: ");
        string password = HidePasswordInput();
        if (username == null)
        {
            PrintError("Ingen brugernavn indtastet");
            Console.WriteLine("Tryk på en tast for at fortsætte...");
            Console.ReadKey();
        }
        if (!_auth.Login(username, password))
        {

            PrintError($"Forkert brugernavn '{username}' eller adgangskode");
            Console.WriteLine("Tryk på en tast for at fortsætte...");
            Console.ReadKey();
        }
        else
        {
            CurrentUser = username;
        }
    }

    #region Is methods

    /// <summary>
    /// Checks if a car's mileage is a palindrome.
    /// </summary>
    /// <param name="car">The car object containing the mileage information.</param>
    /// <returns>True if the mileage is a palindrome, otherwise false.</returns>
    static bool IsPalindrome(Car car)
    {
        string odometer = car.Mileage.ToString() ?? string.Empty;

        for (int i = 0; i < odometer.Length / 2; i++)
        {
            if (odometer[i] != odometer[odometer.Length - i - 1])
            {
                return false;
            }
        }
        return true;
    }

    static bool DoFuelTypeIdExists(int fuelTypeId)
    {
        return Enum.IsDefined(typeof(Engine.FuelType), fuelTypeId);
    }

    #endregion Is methods
    #region Car user interaction methods

    /// <summary>
    /// Selects a car from the list of cars.
    /// </summary>
    /// <returns>The selected car object.</returns>
    static Car? SelectCar()
    {
        List<MenuItem> menuItems = new List<MenuItem>();

        int i = 0;
        foreach (Car car in _carList.GetCars())
        {
            menuItems.Add(new MenuItem($"{car.Brand} {car.Model} {car.Year}", i));
            i++;
        }

        Header("Vælg bil");

        int selectedIndex = PaganizesMenu(menuItems, 10);
        if (selectedIndex == -1) { return null; }
        return _carList.GetCars()[selectedIndex];
    }

    /// <summary>
    /// Input a new car to add to the list.
    /// </summary>
    /// <returns>A car object</returns>
    static Car InputAddCar()
    {
        string? brand; // Brand of the car
        string? model; // Model of the car
        int year; // Year of the car
        char gearType; // Gear type as a character
        int mileage; // Mileage of the car
        int fuelTypeId; // Fuel type ID of the car
        double fuelEfficiency = 0f; // Fuel efficiency of the car
        string description = ""; // Description of the car
        OwnerList ownerList = OwnerList.Instance; // Owner list object

        // Engine
        string engineName;
        double engineCcm;
        int engineHorsePower;
        int engineTorque;
        Engine.FuelType? engineFuel = null;
        int engineMileage;
        DateTime engineLastService;
        int engineServiceIntervalMileage;
        int engineServiceIntervalMonths;

        // Tire
        string tireBrand;
        string tireModel;
        int tireWidth;
        int tireHeight;
        int tireInch;
        Tire.ConstructionType? tireConstruction = null;
        Tire.SeasonType? tireSeason = null;

        Console.Clear(); // Clear the console window
        Console.CursorVisible = true;

        Header("Tilføj bil"); // Display the header

        do
        {
            Console.Write("Mærke: ");
            brand = Console.ReadLine(); // Read the brand from the console
            if (string.IsNullOrEmpty(brand)) // If the brand is empty
            {
                PrintError("Mærke må ikke være tomt."); // Display an error message
            }
        } while (string.IsNullOrEmpty(brand)); // Repeat until a valid brand is entered

        do
        {
            Console.Write("Model: ");
            model = Console.ReadLine(); // Read the model from the console
            if (string.IsNullOrEmpty(model)) // If the model is empty
            {
                PrintError("Model må ikke være tomt."); // Display an error message
            }
        } while (string.IsNullOrEmpty(model)); // Repeat until a valid model is entered

        do
        {
            Console.Write("Årgang: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out year)) // Read the year from the console
            {
                PrintError("Årgang skal være et tal."); // Display an error message
            }
            else if (year < FirstAutomobileYear) // If the year is before the first car
            {
                PrintError($"Årgang skal være efter {FirstAutomobileYear}."); // Display an error message
            }
        } while (year < FirstAutomobileYear); // Repeat until a valid year is entered

        do
        {
            Console.Write("Gear type (A/M): ");
            gearType = char.ToUpper(Console.ReadKey().KeyChar); // Read the gear type from the console
            Console.WriteLine(); // Move the cursor to the next line
            if (gearType != 'A' && gearType != 'M') // If the gear type is not A or M
            {
                PrintError("Gear type skal være enten A eller M."); // Display an error message
            }
        } while (gearType != 'A' && gearType != 'M'); // Repeat until a valid gear type is entered

        do
        {
            Console.Write("Kilometer: ");
            string? input = Console.ReadLine(); // Read the mileage from the console
            if (!int.TryParse(input, out mileage)) // If the mileage is not a number
            {
                PrintError("Kilometer skal være et tal."); // Display an error message
            }
        } while (mileage == 0); // Repeat until a valid mileage is entered

        do
        {
            Console.WriteLine("Kilometer per liter:"); // Display the fuel efficiencies
            string? input = Console.ReadLine();
            if (!double.TryParse(input, out fuelEfficiency)) // If the fuel efficiency is not a number
            {
                PrintError("Kilometer per liter skal være et tal."); // Display an error message
            }
        } while (fuelEfficiency == 0); // Repeat until a valid fuel efficiency is entered

        do
        {
            Console.Write("Beskrivelse: ");
            description = Console.ReadLine() ?? string.Empty; // Read the description from the console
        } while (description == null); // Repeat until a valid description is entered

        // Engine
        do
        {
            Console.Write("Motor navn: ");
            engineName = Console.ReadLine();
            if (string.IsNullOrEmpty(engineName))
            {
                PrintError("Motor navn må ikke være tomt.");
            }
        } while (string.IsNullOrEmpty(engineName));

        do
        {
            Console.Write("Motor ccm: ");
            string? input = Console.ReadLine();
            if (!double.TryParse(input, out engineCcm))
            {
                PrintError("Motor ccm skal være et tal.");
            }
        } while (engineCcm == 0);

        do
        {
            Console.Write("Motor hestekræfter: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out engineHorsePower))
            {
                PrintError("Motor hestekræfter skal være et tal.");
            }
        } while (engineHorsePower == 0);

        do
        {
            Console.Write("Motor drejningsmoment: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out engineTorque))
            {
                PrintError("Motor drejningsmoment skal være et tal.");
            }
        } while (engineTorque == 0);

        do
        {
            Console.WriteLine("Brændstoftyper:");
            foreach (Engine.FuelType fuelType in Enum.GetValues(typeof(Engine.FuelType)))
            {
                Console.WriteLine($"{(int)fuelType}: {fuelType}");
            }
            Console.Write("Brændstoftype: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out fuelTypeId) || !Enum.IsDefined(typeof(Engine.FuelType), fuelTypeId))
            {
                PrintError("Brændstoftype skal være et gyldigt tal.");
            }
            else
            {
                engineFuel = (Engine.FuelType)fuelTypeId;
            }
        } while (engineFuel == null);

        do
        {
            Console.Write("Motor kilometerstand: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out engineMileage))
            {
                PrintError("Motor kilometerstand skal være et tal.");
            }
        } while (engineMileage == 0);

        do
        {
            Console.Write("Sidste service (dd-MM-yyyy): ");
            string? input = Console.ReadLine();
            if (!DateTime.TryParse(input, out engineLastService))
            {
                PrintError("Sidste service skal være en gyldig dato.");
            }
        } while (engineLastService == DateTime.MinValue);

        do
        {
            Console.Write("Service interval (kilometer): ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out engineServiceIntervalMileage))
            {
                PrintError("Service interval skal være et tal.");
            }
        } while (engineServiceIntervalMileage == 0);

        do
        {
            Console.Write("Service interval (måneder): ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out engineServiceIntervalMonths))
            {
                PrintError("Service interval skal være et tal.");
            }
        } while (engineServiceIntervalMonths == 0);

        // Tire
        do
        {
            Console.Write("Dæk mærke: ");
            tireBrand = Console.ReadLine();
            if (string.IsNullOrEmpty(tireBrand))
            {
                PrintError("Dæk mærke må ikke være tomt.");
            }
        } while (string.IsNullOrEmpty(tireBrand));

        do
        {
            Console.Write("Dæk model: ");
            tireModel = Console.ReadLine();
            if (string.IsNullOrEmpty(tireModel))
            {
                PrintError("Dæk model må ikke være tomt.");
            }
        } while (string.IsNullOrEmpty(tireModel));

        do
        {
            Console.Write("Dæk bredde: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out tireWidth))
            {
                PrintError("Dæk bredde skal være et tal.");
            }
        } while (tireWidth == 0);

        do
        {
            Console.Write("Dæk højde: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out tireHeight))
            {
                PrintError("Dæk højde skal være et tal.");
            }
        } while (tireHeight == 0);

        do
        {
            Console.Write("Dæk tommer: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out tireInch))
            {
                PrintError("Dæk tommer skal være et tal.");
            }
        } while (tireInch == 0);

        do
        {
            Console.WriteLine("Dæk konstruktionstyper:");
            foreach (Tire.ConstructionType constructionType in Enum.GetValues(typeof(Tire.ConstructionType)))
            {
                Console.WriteLine($"{(int)constructionType}: {constructionType}");
            }
            Console.Write("Dæk konstruktionstype: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out int constructionTypeId) || !Enum.IsDefined(typeof(Tire.ConstructionType), constructionTypeId))
            {
                PrintError("Dæk konstruktionstype skal være et gyldigt tal.");
            }
            else
            {
                tireConstruction = (Tire.ConstructionType)constructionTypeId;
            }
        } while (!Enum.IsDefined(typeof(Tire.ConstructionType), tireConstruction));

        do
        {
            Console.WriteLine("Dæk sæsontyper:");
            foreach (Tire.SeasonType seasonType in Enum.GetValues(typeof(Tire.SeasonType)))
            {
                Console.WriteLine($"{(int)seasonType}: {seasonType}");
            }
            Console.Write("Dæk sæsontype: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out int seasonTypeId) || !Enum.IsDefined(typeof(Tire.SeasonType), seasonTypeId))
            {
                PrintError("Dæk sæsontype skal være et gyldigt tal.");
            }
            else
            {
                tireSeason = (Tire.SeasonType)seasonTypeId;
            }
        } while (!Enum.IsDefined(typeof(Tire.SeasonType), tireSeason));

        Car car = new Car(
            id: _carList.GenerateId(),
            brand: brand,
            model: model,
            year: year,
            gearType: gearType,
            //fuel: (Engine.FuelType)fuelTypeId,
            fuelEfficiency: fuelEfficiency,
            mileage: mileage,
            engine: new Engine(
                name: "1.6",
                ccm: 1600,
                horsePower: 120,
                torque: 200,
                fuel: Engine.FuelType.Benzin,
                mileage: 100000, // Mileage
                lastService: DateTime.Now, // LastService
                serviceIntervalMileage: 15000, // ServiceIntervalMileage
                serviceIntervalMonths: 12 // ServiceIntervalMonths
            ),
                Wheel.GetSetOf4Wheels(
                    new Tire(
                        brand: "Bridgestone",
                        model: "Turanza",
                        width: 205,
                        height: 55,
                        inch: 16,
                        construction: Tire.ConstructionType.Radial,
                        season: Tire.SeasonType.Summer
                        )
                    ),
                description: "A nice car",
                owner: ownerList.GetOwnerById(0)
            ); // Create a new car object

        return car; // Return the car object
    }

    /// <summary>
    /// Removes a car from the list.
    /// </summary>
    /// <param name="selectedCar"></param>
    static void RemoveCar(Car selectedCar)
    {
        _carList.Remove(selectedCar);
        _selectedCar = null;
    }

    /// <summary>
    /// Checks if the mileage of a car is a palindrome.
    /// </summary>
    /// <param name="selectedCar">The car object to check.</param>
    static void PalinDrome(Car selectedCar)
    {
        Console.Clear();
        if (IsPalindrome(selectedCar))
        {
            Console.WriteLine("Kilometer tallet er et palindrom");
        }
        else
        {
            Console.WriteLine("Kilometer tallet er ikke et palindrom");
        }
        Console.WriteLine("\nTryk på en tast for at fortsætte...");
        Console.Write(Console.ReadKey());

    }

    /// <summary>
    /// Calculates the fuel needed for a trip.
    /// </summary>
    /// <param name="selectedCar">The car object to calculate the fuel needed for.</param>
    /// <param name="distance">The distance of the trip.</param>
    /// <returns>The fuel needed for the trip.</returns>
    static double CalculateFuelNeeded(Car selectedCar, double distance)
    {
        double fuelNeeded = distance / selectedCar.FuelEfficiency;
        return fuelNeeded;
    }

    /// <summary>
    /// Calculates the cost of a trip.
    /// </summary>
    /// <param name="selectedCar">The car object to calculate the cost for.</param>
    static void CalculateTripCost(Car selectedCar)
    {
        Console.Clear();
        Console.WriteLine("Indtast kilometer for turen:");
        string input = Console.ReadLine();
        if (!double.TryParse(input, out double distance))
        {
            PrintError("Kilometer skal være et tal");
            Console.WriteLine("\nTryk på en tast for at fortsætte...");
            Console.ReadKey();
            return;
        }
        double fuelNeeded = CalculateFuelNeeded(selectedCar, distance);
        decimal fuelPrice = (decimal)(_fuelPricelist.FuelPrices.Find(f => f.FuelType == selectedCar.Engine?.Fuel)?.Price ?? 0);
        decimal tripCost = (decimal)fuelNeeded * fuelPrice;
        if (selectedCar.IsEngineRunning)
        {
            selectedCar.UpdateMileAge((int)distance);
        }
        Console.WriteLine($"Pris for turen: {tripCost:F2} kr");
        Console.WriteLine("\nTryk på en tast for at fortsætte...");
        Console.ReadKey();
    }

    static void InputTour(Car selectedCar)
    {
        string? input;
        Trip trip;
        DateTime date;
        Console.Clear();
        if (selectedCar.IsEngineRunning)
            Header("Kør en tur");
        else
        {
            Header("Kør en tur simuleret");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Start motoren for at køre en tur\n");
            Console.ResetColor();
        }
        Console.Write($"Intast dato for turen (default '{DateTime.Now.ToString()}': ");
        input = Console.ReadLine();
        if (string.IsNullOrEmpty(input))
        {
            date = DateTime.Now;
        }
        else if (!DateTime.TryParse(input, out date))
        {
            PrintError("Dato skal være et tal");
            PrintError("Format: dd-MM-yyyy");
            Console.WriteLine("\nTryk på en tast for at fortsætte...");
            Console.ReadKey();
            return;
        }

        Console.Write("Indtast kilometer for turen: ");
        input = Console.ReadLine();
        if (!double.TryParse(input, out double distance))
        {
            PrintError("Kilometer skal være et tal");
            Console.WriteLine("\nTryk på en tast for at fortsætte...");
            Console.ReadKey();
            return;
        }

        Console.Write("Indtast start tidspunkt for turen: ");
        input = Console.ReadLine();
        if (!TimeSpan.TryParse(input, out TimeSpan startTimeSpan))
        {
            PrintError("Start tidspunkt skal være et tal");
            PrintError("Format: HH:mm");
            Console.WriteLine("\nTryk på en tast for at fortsætte...");
            Console.ReadKey();
            return;
        }
        DateTime startTime = date.Add(startTimeSpan);

        Console.Write("Indtast slut tidspunkt for ture: ");
        input = Console.ReadLine();
        if (!TimeSpan.TryParse(input, out TimeSpan endTimeSpan))
        {
            PrintError("slut tidspunkt skal være et tal");
            PrintError("Format: HH:mm");
            Console.WriteLine("\nTryk på en tast for at fortsætte...");
            Console.ReadKey();
            return;
        }
        DateTime endTime = date.Add(endTimeSpan);
        double fuelPrice;

        if (selectedCar.Engine?.Fuel != null)
        {
            Console.Write($"Prisen på {selectedCar.Engine.Fuel} default '{FuelPriceList.Instance.FuelPrices.Find(f => f.FuelType == selectedCar.Engine.Fuel)?.Price}': ");
            input = Console.ReadLine();
            if (input == null || input == "")
            {
                fuelPrice = _fuelPricelist.GetPrice(selectedCar.Engine.Fuel) ?? 0f;
            }
            else if (!double.TryParse(input, out fuelPrice))
                fuelPrice = _fuelPricelist.GetPrice(selectedCar.Engine.Fuel) ?? 0f;
            {
                PrintError("Prisen skal være et tal!");
                Console.WriteLine("\nTryk på en tast for at fortsætte...");
                Console.ReadKey();
                return;
            }
        }
        else
        {
            fuelPrice = 0;
        }
        trip = new(distance, date, startTime, endTime, fuelPrice);
        _selectedCar?.Trips?.Append(trip);
        TimeSpan duration = trip.CalculateDuration();
        Console.WriteLine($"{trip.GetTripInfo(selectedCar)}");
        Console.WriteLine("\nTryk på en tast for at fortsætte...");
        Console.ReadKey();
    }

    #endregion Car user interaction methods
    #region Table methods

    /// <summary>
    /// Creates a horizontal table frame for the console output.
    /// </summary>
    /// <param name="columns">A list of integers representing the width of each column.</param>
    static void CreateTableFrameH(List<int> columns)
    {
        Console.Write("+");
        foreach (int column in columns)
        {
            Console.Write(new string('-', column + 2));
            Console.Write("+");
        }
        Console.WriteLine();
    }

    #endregion Table methods
    #region String methods

    /// <summary>
    /// Displays a title with the specified text.
    /// </summary>
    public static void Header(string subTitle, string title = $"{Constants.carAppTitle} {Constants.carAppVersion}")
    {
        Console.Clear();
        int l = Math.Max(title.Length, subTitle.Length) + 3;
        Frame frame = new Frame(l, 2);
        frame.SetFrameText(title);
        frame.Render();
        Console.WriteLine();
        Console.WriteLine(CenterString(subTitle, l));
        Console.WriteLine(new string('-', l + 1));
        Console.WriteLine();
    }

    /// <summary>
    /// Centers the given text within a specified width.
    /// </summary>
    /// <param name="text">The text to center.</param>
    /// <param name="width">The width within which to center the text.</param>
    /// <returns>The centered text with padding.</returns>
    static string CenterString(string text, int width)
    {
        if (width <= text.Length)
        {
            return text; // Or throw an exception, or truncate the string
        }
        int padding = width - text.Length;
        int leftPadding = padding / 2;
        int rightPadding = padding - leftPadding;
        return new string(' ', leftPadding) + text + new string(' ', rightPadding);
    }

    /// <summary>
    /// Prints an error message in red text.
    /// </summary>
    /// <param name="message"></param>
    static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Hide the password input.
    /// </summary>
    /// <returns>Password user entered</returns>
    static public string HidePasswordInput()
    {
        string pass = string.Empty;
        ConsoleKey key;
        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && pass.Length > 0)
            {
                Console.Write("\b \b");
                pass = pass[0..^1];
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                Console.Write("*");
                pass += keyInfo.KeyChar;
            }
        } while (key != ConsoleKey.Enter);
        return pass;
    }

    #endregion String methods
    #region Rapport methods

    /// <summary>
    /// Displays a report of the car's information.
    /// </summary>
    /// <param name="car">The car object to display the report for.</param>
    static void ShowCarDetail(Car car)
    {
        Console.Clear();
        Header("Bilrapport"); // Display the header
        Console.WriteLine();
        Console.WriteLine(
            $"Bilmærke: {car.Brand}" + "\n" +
            $"Bilmodel: {car.Model}" + "\n" +
            $"Årgang: {car.Year}" + "\n" +
            $"Gear: {car.GearType}" + "\n" +
            $"Brændstof: {car.Engine?.Fuel}" + "\n" +
            $"Forbrug: {car.FuelEfficiency} km/l" + "\n" +
            $"Kilometerstand: {car.Mileage}" +
            (IsPalindrome(car) ? " ** Palindrome nummer **" : "") + "\n" + // Check if the mileage is a palindrome
            $"Beskrivelse: {car.Description}" + "\n" +
            (car.IsEngineRunning ? "Bilen er tændt" : "Bilen er slukket") + "\n"
            );
        Console.WriteLine("\nTryk på en tast for at fortsætte...");
        Console.Write(Console.ReadKey());
    }

    /// <summary>
    /// Displays a list of cars in a table format.
    /// </summary>
    static void ShowCarList()
    {
        List<int> columns = new() { 3, 20, 20, 8, 20 }; // number of columns and their width
        int pageIndex = 0;
        int pageSize = 10;
        int totalPages = (int)Math.Ceiling(_carList.GetCars().Count / (double)pageSize);
        string errorMessage = "";

        while (true)
        {
            Console.Clear();
            Header("Biloversigt");

            // Create Table for console
            CreateTableFrameH(columns); // Create a horizontal table frame
            Console.WriteLine(
                "| " + "#".PadLeft(columns[0]) +
                " | " + CenterString("Mærke", columns[1]) +
                " | " + CenterString("Model", columns[2]) +
                " | " + CenterString("Årgang", columns[3]) +
                " | " + CenterString("Kilemetertal", columns[4]) +
                " |"
                );
            CreateTableFrameH(columns); // Create a horizontal table frame

            var pagedCars = _carList.GetCars()
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();

            for (int i = 0; i < pagedCars.Count; i++)
            {
                Console.WriteLine(
                    "| " + $"{pagedCars[i].Id}".PadLeft(columns[0]) +
                    " | " + $"{pagedCars[i].Brand}".PadRight(columns[1]) +
                    " | " + $"{pagedCars[i].Model}".PadRight(columns[2]) +
                    " | " + $"{pagedCars[i].Year}".PadRight(columns[3]) +
                    " | " + $"{pagedCars[i].Mileage}".PadLeft(columns[4]) +
                    " |");

                CreateTableFrameH(columns); // Create a horizontal table frame
            }

            if (pageIndex > 0)
            {
                Console.WriteLine("F11 previous page");
            }
            if (totalPages > pageIndex + 1)
            {
                Console.WriteLine("F12 next page");
            }
            Console.WriteLine("ESC: Afslut");
            Console.WriteLine($"\nSide {pageIndex + 1} af {totalPages}");

            if (errorMessage != "")
            {
                var Position = Console.GetCursorPosition();
                Console.SetCursorPosition(0, Position.Top + 2);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errorMessage);
                Console.ResetColor();
                Console.SetCursorPosition(Position.Left, Position.Top);
                errorMessage = "";
            }

            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Escape)
            {
                return;
            }
            else if (key.Key == ConsoleKey.F12 && pageIndex < totalPages - 1)
            {
                pageIndex++;
            }
            else if (key.Key == ConsoleKey.F11 && pageIndex > 0)
            {
                pageIndex--;
            }
            else
            {
                errorMessage = $"Fejl: Gyldige taster er F1-F{pagedCars.Count}.";
            }
        }
    }

    /// <summary>
    /// List cars by owners.
    /// </summary>
    static void ShowCarListByOwners()
    {
        Console.Clear();
        Header("Ejer og deres biler");
        Console.WriteLine();

        var carsByOwner = _carList.GetCars()
            .Where(car => car.Owner != null)
            .GroupBy(car => car.Owner)
            .OrderBy(group => group.Key.Name);

        foreach (var ownerGroup in carsByOwner)
        {
            Console.WriteLine($"Ejer: {ownerGroup.Key.Name}");
            foreach (var car in ownerGroup)
            {
                Console.WriteLine($"- {car.Brand} {car.Model} {car.Year}");
            }
            Console.WriteLine();
        }
        Console.WriteLine("\nTryk på en tast for at fortsætte...");
        Console.Write(Console.ReadKey());
    }

    #endregion Rapport methods
    #region Menu methods

    /// <summary>
    /// Displays the main menu and handles the user's choice.
    /// </summary>
    /// <returns>True if the user wants to exit, otherwise false.</returns>
    static void Menu()
    {
        string errorMessage = "";
        do
        {
            Role role = _auth.GetRole(CurrentUser);
            List<MenuItem> menuItems = new List<MenuItem> { };
            if (role == Role.Guest)
                menuItems.Add(new MenuItem("Log ind", 8));
            else
                menuItems.Add(new MenuItem("Log ud", 6));
            if (role != Role.Guest)
                menuItems.Add(new MenuItem("Rapport", 9));
            menuItems.Add(new MenuItem("Vis liste af biler", 5));
            if (_auth.User != null) menuItems.Add(new MenuItem("Vælg bil", 0));
            if (_auth.User != null) menuItems.Add(new MenuItem("Tilføj bil", 1));
            if (_selectedCar != null && role != Role.Guest)
            {
                if (role == Role.Admin)
                {
                    menuItems.Add(new MenuItem($"Fjern bilen", 2));
                }
                menuItems.Add(new MenuItem("Bil detaljer", 3));
                if (_selectedCar.IsEngineRunning)
                {
                    menuItems.Add(new MenuItem("Stop motor", 4));
                }
                else
                {
                    menuItems.Add(new MenuItem("Start motor", 4));
                }
                menuItems.Add(new MenuItem("Kør en tur", 7));
                menuItems.Add(new MenuItem("Er kilometer tal et palindrom?", 22));
            }

            Console.Clear();
            Header("Main Menu");

            if (_selectedCar != null)
            {
                Console.WriteLine($"Bil der er valgt er en: {_selectedCar.Brand} {_selectedCar.Model} med id {_selectedCar.Id}");
            }
            Console.WriteLine();

            int selectedIndex = PaganizesMenu(menuItems, 10);

            Console.SetCursorPosition(0, Console.CursorTop + 1);
            PrintError(errorMessage);

            int CTop = Console.CursorTop;

            switch (selectedIndex)
            {
                case 0:
                    _selectedCar = SelectCar();
                    break;
                case 1:
                    _carList.Add(InputAddCar());
                    break;
                case 2:
                    RemoveCar(_selectedCar);
                    break;
                case 3:
                    ShowCarDetail(_selectedCar);
                    break;
                case 4:
                    _selectedCar?.ToggleEngine();
                    break;
                case 5:
                    ShowCarList();
                    break;
                case 8:
                    Login();
                    break;
                case 6:
                    _auth.Logout();
                    break;
                case 7:
                    InputTour(_selectedCar);
                    break;
                case 9:
                    RapportMenu();
                    break;
                case 22:
                    PalinDrome(_selectedCar);
                    break;
                case -1:
                    return;
                default:
                    errorMessage = "Ugyldig valg";
                    break;
            }
        } while (true);
    }

    /// <summary>
    /// Menu for showing reports.
    /// </summary>
    static void RapportMenu()
    {
        string errorMessage = "";
        do
        {
            List<MenuItem> menuItems = new List<MenuItem> { };
            menuItems.Add(new MenuItem("List ejer og deres biler", 0));

            Console.Clear();
            Header("Rapport Menu");

            int selectedIndex = PaganizesMenu(menuItems, 10);

            Console.SetCursorPosition(0, Console.CursorTop + 1);
            PrintError(errorMessage);

            int CTop = Console.CursorTop;

            switch (selectedIndex)
            {
                case 0:
                    ShowCarListByOwners();
                    break;
                case -1:
                    return;
                default:
                    errorMessage = "Ugyldig valg";
                    break;
            }
        } while (true);
    }

    /// <summary>
    /// Paginates a list of menu items.
    /// </summary>
    /// <param name="menuItems"></param>
    /// <param name="pageSize"></param>
    /// <param name="sorting"></param>
    /// <returns></returns>
    static int PaganizesMenu(List<MenuItem> menuItems, int pageSize, bool sorting = false)
    {
        string errorMessage = "";
        int pageIndex = 0;
        (int Left, int Top) Position;
        Console.CursorVisible = false;

        int totalPages = (int)Math.Ceiling(menuItems.Count / (double)pageSize);

        if (sorting)
        {
            menuItems = menuItems.OrderBy(m => m.Name).ToList();
        }

        var pagedMenuItems = menuItems
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select((item, index) => $"F{index + 1}: {item.Name}")
            .ToList();

        for (int i = 0; i < pagedMenuItems.Count; i++)
        {
            Console.WriteLine(pagedMenuItems[i], Console.WindowWidth);
        }

        Console.WriteLine();

        if (pageIndex > 0)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("F11 ");
            Console.ResetColor();
            Console.Write("previous page, ");
        }

        if (totalPages > pageIndex + 1)
        {
            Console.WriteLine("F12 next page, ");
        }

        Console.WriteLine("ESC: Afslut");

        Console.WriteLine($"\nSide {pageIndex + 1} af {totalPages}");

        if (errorMessage != "")
        {
            Position = Console.GetCursorPosition();
            Console.SetCursorPosition(0, Position.Top + 2);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();
            Console.SetCursorPosition(Position.Left, Position.Top);
            errorMessage = "";
        }

        var key = Console.ReadKey(true);
        if (key.Key == ConsoleKey.Escape)
        {
            return -1;
        }
        else if (key.Key == ConsoleKey.F12 && pageIndex < totalPages - 1)
        {
            pageIndex++;
        }
        else if (key.Key == ConsoleKey.F11 && pageIndex > 0)
        {
            pageIndex--;
        }
        else if (key.Key >= ConsoleKey.F1 && key.Key <= ConsoleKey.F10)
        {
            int selectedIndex = key.Key - ConsoleKey.F1;
            if (selectedIndex < pagedMenuItems.Count)
            {

                return menuItems[selectedIndex + (pageIndex * pageSize)].Index;
            }
            else
            {
                errorMessage = "Fejl. Ikke gyldig valg";
            }
        }
        else
        {
            errorMessage = $"Fejl: Gyldige taster er F1-F{pagedMenuItems.Count}.";
        }
        return -2;
    }

    #endregion Menu methods

    /// <summary>
    /// The main entry point of the program.
    /// Handles the login and menu logic.
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        // Set the console output encoding to UTF-8 so æøå are displayed correctly
        Console.OutputEncoding = Encoding.UTF8;

        // Get the current OS user
        string currentUser = Environment.UserName;
        Console.WriteLine($"Current OS User: {currentUser}");

        Menu();
    }
}
