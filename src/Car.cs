using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoKomis {
    public class Car {
        public enum FuelType {
            Benzyna,
            Diesel,
            Elektryczny,
            Hybryda
        }

        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public uint Mileage { get; set; }
        public string Fuel { get; set; }
        public decimal Price { get; set; }
        public byte[]? ImageData { get; set; }

        public Car() {
            this.Brand = string.Empty;
            this.Model = string.Empty;
            this.Year = 0;
            this.Mileage = 0;
            this.Fuel = string.Empty;
            this.Price = 0;
            this.ImageData = null;
        }

        public Car(
            string brand,
            string model,
            ushort year,
            uint mileage,
            FuelType fuel,
            decimal price,
            string imagePath = ""
        ) {
            this.Brand = brand;
            this.Model = model;
            this.Year = year;
            this.Mileage = mileage;
            this.Fuel = fuel.ToString();
            this.Price = price;
            this.ImageData = string.IsNullOrEmpty(imagePath) ? null : File.ReadAllBytes(imagePath);
        }
        
        public string FullName => $"{Brand} {Model}";
        public string Details => $"{Year} • {Mileage} km • {Fuel}";
        public string PriceText => $"{Price:N0} PLN";
    }
}
