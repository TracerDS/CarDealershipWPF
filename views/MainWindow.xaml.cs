using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace AutoKomis
{
    public partial class MainWindow : Window
    {
        private List<Car> allCars = new();
        private bool isInitialized;
        private readonly SqliteDatabase database = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            UpdateMaxPrice();

            MainWindowBox.ItemsSource = allCars;
            isInitialized = true;
        }

        private void LoadData()
        {
            try
            {
                allCars = database.GetAllCars();

                // Jeśli baza jest pusta, spróbuj załadować z JSON jako fallback
                if (allCars.Count == 0)
                {
                    LoadFromJsonIfAvailable();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    $"Nie można wczytać danych z bazy: {exc.Message}",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void LoadFromJsonIfAvailable()
        {
            string jsonFilePath = "resources/cars.json";

            if (!File.Exists(jsonFilePath))
                return;

            try
            {
                string jsonString = File.ReadAllText(jsonFilePath);
                var carsFromJson = JsonSerializer.Deserialize<List<Car>>(jsonString);

                if (carsFromJson != null && carsFromJson.Count > 0)
                {
                    foreach (var car in carsFromJson)
                    {
                        database.AddCar(car);
                    }
                    allCars = carsFromJson;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    $"Nie można wczytać danych z pliku JSON: {exc.Message}",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void SaveToJson()
        {
            string jsonFilePath = "cars.json";

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(allCars, options);
                File.WriteAllText(jsonFilePath, jsonString);
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    $"Nie można zapisać danych do pliku JSON: {exc.Message}",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void SaveToDatabase()
        {
            try
            {
                // Baza automatycznie zapisuje podczas dodawania
                MessageBox.Show("Dane są automatycznie zapisywane w bazie SQLite!", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    $"Błąd podczas zapisywania: {exc.Message}",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void UpdateMaxPrice()
        {
            decimal maxPrice = decimal.MinValue;
            foreach (var car in allCars)
            {
                if (car.Price > maxPrice)
                {
                    maxPrice = car.Price;
                }
            }
            if (maxPrice > (decimal)MaxPriceSlider.Maximum)
            {
                MaxPriceSlider.Maximum = (double)maxPrice;
            }
        }

        private void ApplyFilters()
        {
            if (!isInitialized)
                return;
            if (ModelComboBox.SelectedItem == null)
                return;
            if (FuelTypeComboBox.SelectedItem == null)
                return;

            Debug.Assert(allCars != null);

            var selectedItem = (ComboBoxItem)ModelComboBox.SelectedItem;
            var selectedBrand = selectedItem.Content.ToString();
            var maxPrice = MaxPriceSlider.Value;
            var selectedFuelItem = (ComboBoxItem)FuelTypeComboBox.SelectedItem;
            var selectedFuel = selectedFuelItem.Content.ToString();

            var filteredCars = allCars;

            if (selectedBrand != "Wszystkie")
            {
                filteredCars = filteredCars.FindAll(car => car.Brand == selectedBrand);
            }

            filteredCars = filteredCars.FindAll(car => car.Price <= (decimal)maxPrice);

            if (selectedFuel != "Wszystkie")
            {
                filteredCars = filteredCars.FindAll(car => car.Fuel == selectedFuel);
            }

            MainWindowBox.ItemsSource = filteredCars;
        }

        private void AddCarBtn_Click(object sender, RoutedEventArgs e)
        {
            var addCarWindow = new AddCarWindow();
            if (addCarWindow.ShowDialog() == true && addCarWindow.NewCar != null)
            {
                try
                {
                    database.AddCar(addCarWindow.NewCar);
                    allCars.Add(addCarWindow.NewCar);
                    UpdateMaxPrice();
                    ApplyFilters();

                    MessageBox.Show("Pojazd został dodany pomyślnie do bazy danych!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception exc)
                {
                    MessageBox.Show($"Błąd podczas dodawania pojazdu: {exc.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveDatabaseBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveToDatabase();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ModelComboBox_Change(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void MaxPriceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyFilters();
        }

        private void FuelTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }
    }
}
