//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ThunderITforGEA.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Klient
    {
        public string Id_k { get; set; }
        [Display(Name = "Imi�")]
        public string imie { get; set; }
        [Display(Name = "Nazwisko")]
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
        public string telefon { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        public string Id_k_SG { get; set; }
        public string ilosc_krow { get; set; }
        public string udoj_per_day { get; set; }
        public string wydajnosc_stada { get; set; }
        [Display(Name = "Manager FS")]
        public string manager_fs { get; set; }
        [Display(Name = "Techniczny")]
        public string techniczny { get; set; }
        [Display(Name = "Manager sprzeda�y")]
        public string manager_sprzedazy { get; set; }
        public string typ_obory { get; set; }
        public string obora_dlugosc { get; set; }
        public string obora_szerokosc { get; set; }
        public string obora_rokbudowy { get; set; }
        public string rodzaj_sciolki { get; set; }
        public string typ_obory2 { get; set; }
    
        public virtual ServiceGuard ServiceGuard { get; set; }
    }
}
