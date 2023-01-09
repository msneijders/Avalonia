using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ListViewApp.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        ObservableCollection<OrderregContainer>? regels;

        [ObservableProperty]
        OrderregContainer? selected;

        static OrderregContainer Empty = new OrderregContainer()
        {
            Item = new Orderreg()
        };

        public MainViewModel()
        {
            
            List<OrderregContainer> containers = new List<OrderregContainer>();
            for(int i = 1; i < 900; i++)
            {
                containers.Add(new OrderregContainer()
                {
                    Item = new Orderreg()
                    {
                        Omschrijving = $"Omschrijving {i}",
                        S1 = Random.Shared.Next(1, 99),
                        S2 = Random.Shared.Next(1, 99),
                        S3 = Random.Shared.Next(1, 99),
                        Prijs = Math.Round(Random.Shared.NextDouble() * 100.0, 2),
                        Aantal = Random.Shared.Next(1, 500),
                        Inhoud = Random.Shared.Next(20, 100),
                        Kolli = Random.Shared.Next(1, 30),
                        Fustcode = Random.Shared.Next(900, 999).ToString(),
                        FustAantal = Random.Shared.Next(1, 20),
                        PartijTotaalAantal = Random.Shared.Next(1, 9999),
                        Kweker = $"Kweker {i}",
                        Verkoopeenheid = Random.Shared.Next(1, 50),
                        TotaalBedrag = Math.Round(Random.Shared.NextDouble() * 100.0, 2)
                    }
                });
            }

            Regels = new ObservableCollection<OrderregContainer>(containers);
        }
    }

    public partial class OrderregContainer : ObservableObject
    {
        [ObservableProperty]
        Orderreg? item;

        public string Sorteringskenmerken => $"{Item?.S1}|{Item?.S2}|{Item?.S3}";
    }

    public class Orderreg
    {
        public string? Omschrijving { get; set; }
        public int S1 { get; set; }
        public int S2 { get; set; }
        public int S3 { get; set; }
        public double Prijs { get; set; }
        public int Aantal { get; set; }
        public int Inhoud { get; set; }
        public int Kolli { get; set; }
        public string? Fustcode { get; set; }
        public int FustAantal { get; set; }
        public int PartijTotaalAantal { get; set; }
        public string? Kweker { get; set; }
        public int Verkoopeenheid { get; set; }
        public double TotaalBedrag { get; set; }
    }
}
