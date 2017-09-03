using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ThunderITforGEA.Models
{
    public class AdminViewModel
    {
       [Display(Name = "Serial Number")]
        public string serialNumber { get; set; }
         [Display(Name = "Imię i nazwisko")]
        public string imieNazwisko { get; set; }
         [Display(Name = "Firma")]
        public string nazwaFirmy { get; set; }
         [Display(Name = "Ile do serwisu")]
        public string ileDoSerwisu { get; set; }
         [Display(Name = "Firma Dealera")]
        public string firmaDealera { get; set; }
         [Display(Name = "Pracownik GEA")]
        public string pracownikGea { get; set; }
         [Display(Name = "Grupa")]
         public string grupa { get; set; }

    }
    public class RegisterKlient
    {

        public string login { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Hasło musi być dłuższe niż {2} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasła nie są identyczne.")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "Imię")]
        [StringLength(100, ErrorMessage = "Imię musi mieć conajmniej {2} litery.", MinimumLength = 2)]
        public string imie { get; set; }
        [Display(Name = "Nazwisko")]
        [Required]
        public string nazwisko { get; set; }
        [Display(Name = "Firma")]
        public string firma { get; set; }
        [Display(Name = "Adres")]
        public string adres { get; set; }
        [Display(Name = "Miasto")]
        public string miasto { get; set; }
        [Display(Name = "Kraj")]
        public string kraj { get; set; }
        [Display(Name = "Telefon")]
        [DataType(DataType.PhoneNumber)]
        public string telefon { get; set; }
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
           [Display(Name = "Przypisz ServiceGuard")]
        public string serialnumber { get; set; }
    }
    public class GEAViewModel
    {
         [Display(Name = "Serial Number")]
        public string serialNumber { get; set; }
         [Display(Name = "Imię i nazwisko")]
        public string imieNazwisko { get; set; }
         [Display(Name = "Nazwa firmy")]
        public string nazwaFirmy { get; set; }
         [Display(Name = "Ile do serwisu")]
        public string ileDoSerwisu { get; set; }
    }
    public class KlientDodajViewModel
    {
        public Klient klient { get; set; }
        public string firmaUzytkownika { get; set; }
    }
    public class DealerViewModel
    {
         [Display(Name = "Serial Number")]
        public string serialNumber { get; set; }
         [Display(Name = "Imię i nazwisko")]
        public string imieNazwisko { get; set; }
         [Display(Name = "Ile do serwisu")]
        public string ileDoSerwisu { get; set; }
        //reszta na tabelke dolna
       //  public string numer_serial { get; set; }
        // public Nullable<System.DateTime> data_ostatniego_serwisu { get; set; }
       //  public string czas_pracy { get; set; }
     //    public string godziny_serwisowe { get; set; }
    }
  
    public class DealerPrzyWyborzeUrzadzenia
    {
        public List<ServiceGuard> SG { get; set; }
         [Display(Name = "Imię nazwisko")]
        public List<string> imieNazwisko { get; set; }
         public List<string> dozwoloneSeriale { get; set; } 
    }
    public class GEAPrzyWyborzeUrzadzenia
    {
        public List<ServiceGuard> SG { get; set; }
        [Display(Name = "Imię nazwisko")]
        public List<string> imieNazwisko { get; set; }
        [Display(Name = "Nazwa firmy")]
        public List<string> firma { get; set; }
      //  public List<string> dozwoloneSeriale { get; set; }
    }
  /*  public class DealerPrzyWyborzeUrzadzenia
    {
        public List<ServiceGuard> SG { get; set; }
        [Display(Name = "Imię nazwisko")]
        public List<AspNetUser> klient { get; set; }
        public List<AspNetUser> dealer { get; set; }
        public List<string> dozwoloneSeriale { get; set; }
    }*/
}