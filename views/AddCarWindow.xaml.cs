using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.Win32;

namespace AutoKomis
{
    public partial class AddCarWindow : Window
    {
        public Car? NewCar { get; private set; }
        private string? selectedImagePath;

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
                string? finalImagePath = null;

                // Jeśli wybrano zdjęcie, skopiuj je do folderu Images
                if (!string.IsNullOrEmpty(selectedImagePath) && File.Exists(selectedImagePath))
                {
                    string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    Directory.CreateDirectory(imagesFolder);

                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(selectedImagePath)}";
                    string destinationPath = Path.Combine(imagesFolder, fileName);

                    File.Copy(selectedImagePath, destinationPath, true);
                    finalImagePath = Path.Combine("Images", fileName);
                }

                NewCar = new Car {
                    Brand = BrandTextBox.Text.Trim(),
                    Model = ModelTextBox.Text.Trim(),
                    Year = int.Parse(YearTextBox.Text),
                    Mileage = uint.Parse(MileageTextBox.Text),
                    Fuel = ((ComboBoxItem) FuelComboBox.SelectedItem).Content.ToString() ?? "Benzyna",
                    Price = decimal.Parse(PriceTextBox.Text),
                    ImageData = finalImagePath != null ? File.ReadAllBytes(finalImagePath) : null,
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

        private void BrowseImageBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenImageFileDialog();
        }

        private void ImageContainer_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenImageFileDialog();
        }

        private void OpenImageFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Wybierz zdjęcie pojazdu",
                Filter = "Pliki obrazów|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Wszystkie pliki|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SetImagePreview(openFileDialog.FileName);
            }
        }

        private void ImageDrop_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ImageDrop_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    string extension = Path.GetExtension(filePath).ToLower();

                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || 
                        extension == ".bmp" || extension == ".gif")
                    {
                        SetImagePreview(filePath);
                    }
                    else
                    {
                        MessageBox.Show("Proszę wybrać plik obrazu (JPG, PNG, BMP, GIF).", 
                                      "Nieprawidłowy format", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void SetImagePreview(string imagePath)
        {
            try
            {
                selectedImagePath = imagePath;
                PreviewImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath));
                PreviewImage.Visibility = Visibility.Visible;
                PlaceholderPanel.Visibility = Visibility.Collapsed;
            }
            catch
            {
                MessageBox.Show("Nie można wczytać obrazu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
