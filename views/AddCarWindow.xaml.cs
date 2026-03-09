using System.Windows;
using System.Windows.Controls;

namespace AutoKomis
{
    public partial class AddCarWindow : Window
    {
        public Car? NewCar { get; private set; }

        public AddCarWindow()
        {
            InitializeComponent();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                MessageBox.Show("Proszę wypełnić wszystkie pola poprawnie.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                NewCar = new Car
                {
                    Brand = BrandTextBox.Text.Trim(),
                    Model = ModelTextBox.Text.Trim(),
                    Year = int.Parse(YearTextBox.Text),
                    Mileage = uint.Parse(MileageTextBox.Text),
                    Fuel = ((ComboBoxItem)FuelComboBox.SelectedItem).Content.ToString() ?? "Benzyna",
                    Price = decimal.Parse(PriceTextBox.Text)
                };

                DialogResult = true;
                Close();
            }
            catch
            {
                MessageBox.Show(
                    "Błąd podczas tworzenia pojazdu. Sprawdź poprawność danych.",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(BrandTextBox.Text))
                return false;

            if (string.IsNullOrWhiteSpace(ModelTextBox.Text))
                return false;

            if (!int.TryParse(YearTextBox.Text, out int year) || year < 1900 || year > 2100)
                return false;

            if (!int.TryParse(MileageTextBox.Text, out int mileage) || mileage < 0)
                return false;

            if (FuelComboBox.SelectedItem == null)
                return false;

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
                return false;

            return true;
        }
    }
}
