using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThunderITforGEA.Models;
using System.Web.Caching;
using System.Net.Mail;
using System.Web.Hosting;
using System.Net;
using ThunderITForGEA.Controllers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ThunderITforGEA.Controllers
{
    public class SerialPortController : Controller
    {
        static bool StopTest=false;     
        static string enter = "\r\n"; //dla czytelności
        public static System.Timers.Timer czytanieSMS;
        static string wpisanyNumerDoTestow;
        static string ctrlZ = char.ConvertFromUtf32(26);
        static string nazwaPortu = "COM1"; //do łatwej zmiany wszędzie
        static int iloscDataBits = 8; //ustawienia wymagane przez podłączony modem
        static Parity parity = Parity.None;
        static StopBits iloscStopBitow = StopBits.One;
        static int clockRate = 115200;
        static SerialPort port = new SerialPort(nazwaPortu, clockRate, parity, iloscDataBits, iloscStopBitow);
        static CacheItemRemovedCallback OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
        static CacheItemRemovedCallback timer = new CacheItemRemovedCallback(interwalOsiagniety);
        static int iloscDniTimer;

         static SerialPortController()  //konstruktor statyczny
        {
            port.RtsEnable = true;  //przy pierwszym odwolaniu do statycznej klasy ustawi te rzeczy
            port.Handshake = Handshake.RequestToSend;
            port.Open();
            czytanieSMS = new System.Timers.Timer();
            czytanieSMS.Interval = 15000;
            czytanieSMS.Elapsed += czytanieSMS_Elapsed;
            port.WriteTimeout = 2000;
            port.ReadTimeout = 2000;
            Task.Delay(200); //niech ma czas na otwarcie
            port.WriteLine("AT+CMGF=1\r\n");  //ustawianie trybu wiadomości przy pierwszym odpaleniu sesji użytkownika
            port.WriteLine("AT\\Q3\r\n");  //ustaw flow control na 3 czyli hardware rts cts, 0 wylacz, 1 software xon
            port.WriteLine("ATE0\r\n");  // wyłącz lokalne echo, czyli odsylanie każdej komendy -50% eventow
            port.DataReceived += port_DataReceived;
            port.ErrorReceived += port_ErrorReceived;         
            AddTask("sprawdzSMS", Convert.ToInt32(6000));
        }

         static void czytanieSMS_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
         {
             Entities baza = new Entities();
             SMS[] sms = baza.SMS.ToArray();
             int iloscLinijek = sms.Length;
             for(int i=0;i<iloscLinijek;i++)
             {
                 przeczytajSMS(i);
             }
             baza.Database.ExecuteSqlCommand("TRUNCATE TABLE [SMS]"); //wyczysc zapisane SMS po przeczytaniu i ogarnieciu
         }

         static void port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
         {
             SerialPort sp = (SerialPort)sender;// raczej kiepsko dziala :P MD
             string odpowiedz = sp.ReadExisting();           
         }
  
        public static void AddTask(string name, int seconds) {
          //  OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved); 
            HttpRuntime.Cache.Insert(name, seconds, null, DateTime.UtcNow.AddMilliseconds(seconds), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, OnCacheRemove);
        }
        public static void interwalOsiagniety(string taskname,object iloscDni, CacheItemRemovedReason removalReason)
        {
            Entities baza = new Entities();          
            foreach(ServiceGuard SG in baza.ServiceGuard.ToList())
            {
                if (!port.IsOpen)
                    port.Open();
                Thread.Sleep(250);
                port.Write("AT+CMGF=1\r");
                Thread.Sleep(250);
                port.WriteLine("AT+CMGS=\"" + SG.nr_tel + "\"\r");
                Thread.Sleep(250);
                port.WriteLine("F" + "\x11" + "C" + '\x001a');  //komenda na wysłanie żądania o aktualnym stanie
                Thread.Sleep(250);
                Biling daneDoZapisu = new Biling();
                Guid s = Guid.NewGuid();
                daneDoZapisu.ServiceGuard_id_sg = SG.serial_number;
                daneDoZapisu.tresc = "F_C";
                daneDoZapisu.Id_b = s.ToString();
                daneDoZapisu.data = DateTime.Now;
                daneDoZapisu.login = "SYSTEM";
                baza.Biling.Add(daneDoZapisu);
                baza.SaveChangesAsync();
            }
            HttpRuntime.Cache.Insert("AktualizacjaInterwal", null, null, DateTime.UtcNow.AddDays(iloscDniTimer), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, timer);
        }
        public void ustawTimer(int interwal)
        {
            iloscDniTimer = interwal;
            HttpRuntime.Cache.Insert("AktualizacjaInterwal", null, null, DateTime.UtcNow.AddDays(iloscDniTimer), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, timer);
        }
     
        public static void CacheItemRemoved(string taskName, object seconds, CacheItemRemovedReason removalReason)
        {    
            for (int i = 1; i <= 20;i++ )
            {
                if(port.IsOpen==true)
                port.Write("AT+CMGR=" + i + "\r\n");            
            }
            Thread.Sleep(2000);
            for (int i = 1; i <= 20; i++)
            {
                if (port.IsOpen == true)
                    port.Write("AT+CMGD=" + i + "\r\n");
            }
                AddTask(taskName, Convert.ToInt32(seconds)); //od nowa za minute to zrob
        }
       
        public void startTesty(string nrTel,string interwal)
        {
            wpisanyNumerDoTestow = nrTel;
            StopTest = false;
            AddTask("InvoiceGenerationDailyTask", Convert.ToInt32(interwal));
        }

        public async Task wyslijEmailAlarm(string nrServiceGuard)
        {
             Entities baza = new Entities();
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            List<AspNetUsers> listaUzytkownikow = sg.AspNetUsers.ToList(); //lista przypisanych do danego SG
            string klient = @"Szanowny Panie <imię> <nazwisko>
Uprzejmie informujemy ,że nadszedł czas okresowego serwisu Pańskiego urządzenia.
Prosimy o kontakt z Naszym Autoryzowanym Dealerem
<firma>
<adres>
<telefon>

Z poważaniem
GEA Polska.";
            string dealer = @"Szanowny Dealerze naszedł czas wykonania serwisu urządzenia podpiętego do ServiceGuard o numerze <serial_number> zainstalowanego u klienta:
<dane klienta>

Z poważaniem
GEA Polska.";
            string gea = @"Nadszedł czas okresowego serwisu urządzenia <serial_number>
Zainstalowanego u klienta
<dane klienta>
Obsługiwanego przez:
<dane dealera>";
            var account = new AccountController();          
            for (int i = 0; i < listaUzytkownikow.Count; i++)
            {
                var rolaUzytkownika = account.UserManager.GetRolesAsync(listaUzytkownikow[i].Id);
                if (rolaUzytkownika.ToString() == "rolnik")
                {
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(listaUzytkownikow[i].Email));
                    message.From = new MailAddress("powiadomienie@gea-ewidencja.pl"); // DO ZMIANY
                    message.Subject = "Witamy w serwisie GEA ServiceGuard!";
                    klient = klient.Replace("<imię> <nazwisko>", listaUzytkownikow[i].imie + " " + listaUzytkownikow[i].nazwisko);
                 //   klient = klient.Replace("<firma>", );
                    message.Body = klient;
                    message.IsBodyHtml = false;
                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "powiadomienie",
                            Password = "cH3StUcac"
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "ssl0.ovh.net";
                        smtp.Port = 25;
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(message);
                    }
                }
                if (rolaUzytkownika.ToString() == "dealer")
                {
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(listaUzytkownikow[i].Email));
                    message.From = new MailAddress("powiadomienie@gea-ewidencja.pl"); // DO ZMIANY
                    message.Subject = "Witamy w serwisie GEA ServiceGuard!";
                    dealer = dealer.Replace("<serial_number>", sg.serial_number);
                //    dealer = dealer.Replace("###HASLO###", haslo);
                    message.Body = dealer;
                    message.IsBodyHtml = false;
                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "powiadomienie",
                            Password = "cH3StUcac"
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "ssl0.ovh.net";
                        smtp.Port = 25;
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(message);
                    }
                }
                if (rolaUzytkownika.ToString() == "admin")
                {

                }
                if (rolaUzytkownika.ToString() == "gea")
                {
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(listaUzytkownikow[i].Email));
                    message.From = new MailAddress("powiadomienie@gea-ewidencja.pl"); // DO ZMIANY
                    message.Subject = "Witamy w serwisie GEA ServiceGuard!";
                    gea = gea.Replace("<serial_number>", sg.serial_number);
                    message.Body = gea;
                    message.IsBodyHtml = false;
                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            UserName = "powiadomienie",
                            Password = "cH3StUcac"
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "ssl0.ovh.net";
                        smtp.Port = 25;
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(message);
                    }
                }
            }
        }
        public static async Task<IList<string>> wypiszRole(string userId)
        {
            using (
                var userManager =
                    new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
            {
                var rolesForUser = await userManager.GetRolesAsync(userId);

                return rolesForUser;
            }
        }
       static void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(1500);  //event strzela na pierwszym bicie, poczekaj na wszystkie dane  
            SerialPort sp = (SerialPort)sender;
            //  sp.Encoding = System.Text.Encoding.UTF8;
            //  sp.Handshake = Handshake.RequestToSend;
            string odpowiedz = sp.ReadExisting();
            Entities baza = new Entities();
           SMS nowyWpis = new SMS();
           nowyWpis.tresc_sms=odpowiedz;
           baza.SMS.Add(nowyWpis);
           baza.SaveChanges();
        }
       public static void przeczytajSMS(int wiersz)
       {
           Entities baza = new Entities();
           SMS[] odpowiedz = baza.SMS.ToArray();
           string aktualnalinijka = odpowiedz[wiersz].tresc_sms;
           string[] dane = aktualnalinijka.Split("\r\n".ToArray());  //dzielenie po enter
           string[] tabelaWiadomosci = new string[dane.Length + 10]; //i+2 niech nie wyjezdza
           int j = 0;
           for (int i = 0; i < dane.Length; i++)
           {
               if (dane[i].Contains("REC READ") == true || dane[i].Contains("REC UNREAD") == true)
               {
                   tabelaWiadomosci[j] = dane[i];
                   tabelaWiadomosci[j + 1] = dane[i + 2]; //dane najpierw maja czas oraz nr, potem null a nastepnie tresc wiadomosci, dlatego +2
                   j = j + 2;
               }
           }
           //czas zapiąć pasy - switch olać w razie nowych komend, które nie będą rozpoznawane po podłodze za serial number
           //guziki
           dane = new string[10];
           for (int i = 1; i < j; i = i + 2) //bylo i=1 w razie czego zmien
           {
               if (tabelaWiadomosci[i].Contains("_") == false)
                   dane = tabelaWiadomosci[i].Split("".ToArray());
               else
                   dane = tabelaWiadomosci[i].Split("_".ToArray());
               try
               {
                   Entities bazaDanych = new Entities();
                   if (dane[1] == "ALARMM")
                   {
                       Alarmy nowyWpis = new Alarmy();
                       ServiceGuard sg = bazaDanych.ServiceGuard.Find(dane[0]);
                       nowyWpis.data_alarmu = DateTime.Now;
                       nowyWpis.status = "do_wykonania";
                       nowyWpis.typ_alarmu = "M";
                       nowyWpis.ServiceGuard_id_sg = sg.serial_number;
                       nowyWpis.serial_number = dane[0];
                       bazaDanych.Alarmy.Add(nowyWpis);
                       bazaDanych.SaveChanges();
                       Thread.Sleep(250);
                       port.Write("AT+CMGF=1\r");
                       Thread.Sleep(250);
                       port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
                       Thread.Sleep(100);
                       port.WriteLine("F" + "\x11" + "OK" + '\x001a');
                       Thread.Sleep(250);
                       //  Entities baza = new Entities();
                       //  ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
                       List<AspNetUsers> listaUzytkownikow = sg.AspNetUsers.ToList(); //lista przypisanych do danego SG
                       for (int k = 0; k < listaUzytkownikow.Count; k++)
                       {
                           port.Write("AT+CMGF=1\r");
                           Thread.Sleep(250);
                           port.WriteLine("AT+CMGS=\"" + listaUzytkownikow[k].telefon + "\"\r");
                           Thread.Sleep(100);
                           var account = new AccountController();
                           var rolaUzytkownika = account.UserManager.GetRoles(listaUzytkownikow[k].Id);
                           string test = "test";
                           if (rolaUzytkownika.ToString() == "rolnik")
                           {
                               port.WriteLine("Szanowny Kliencie uprzejmie informujemy, że nadszedł czas dokonania okresowego serwisu.Prosimy o kontakt z Naszym Dealerem. Zespół GEA Polska." + '\x001a');
                               Thread.Sleep(250);
                           }
                           if (rolaUzytkownika.ToString() == "dealer")
                           {
                               port.WriteLine("Nadszedł czas serwisu urządzenia " + sg.serial_number + ". Zespół GEA Polska." + '\x001a');
                               Thread.Sleep(250);
                           }
                           if (rolaUzytkownika.ToString() == "admin")
                           {

                           }
                           if (rolaUzytkownika.ToString() == "gea")
                           {
                               port.WriteLine("Urządzenie " + sg.serial_number + " wymaga serwisu." + '\x001a');
                               Thread.Sleep(250);
                           }
                           Thread.Sleep(250);
                       }
                   }
                   if (dane[1] == "ALARMD")
                   {
                       ServiceGuard sg = bazaDanych.ServiceGuard.Find(dane[0]);
                       port.Close();
                       Thread.Sleep(500);
                       port.Open();                    
                       port.Write("AT+CMGF=1\r");
                       //   Thread.Sleep(250);
                       port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
                       //    Thread.Sleep(100);
                       port.WriteLine("F" + "\x11" + "OK" + '\x001a');
                       Alarmy nowyWpis = new Alarmy();
                       nowyWpis.data_alarmu = DateTime.Now;
                       nowyWpis.status = "do_wykonania";
                       nowyWpis.typ_alarmu = "D";
                       nowyWpis.ServiceGuard_id_sg = sg.serial_number;
                       nowyWpis.serial_number = dane[0];
                       bazaDanych.Alarmy.Add(nowyWpis);
                       bazaDanych.SaveChanges();                    
                       List<AspNetUsers> listaUzytkownikow = sg.AspNetUsers.ToList(); //lista przypisanych do danego SG
                       for (int l = 0; l < listaUzytkownikow.Count; l++)
                       {
                           port.Write("AT+CMGF=1\r");
                           //     Thread.Sleep(250);
                           port.WriteLine("AT+CMGS=\"" + listaUzytkownikow[l].telefon + "\"\r");
                           //   Thread.Sleep(100);
                           var account = new AccountController();
                           var rolaUzytkownika = account.UserManager.GetRoles(listaUzytkownikow[l].Id);
                           string test = "test";
                           if (rolaUzytkownika.ToString() == "rolnik")
                           {
                               port.WriteLine("Szanowny Kliencie uprzejmie informujemy, że nadszedł czas dokonania okresowego serwisu.Prosimy o kontakt z Naszym Dealerem. Zespół GEA Polska." + '\x001a');
                               //   Thread.Sleep(250);
                           }
                           if (rolaUzytkownika.ToString() == "dealer")
                           {
                               port.WriteLine("Nadszedł czas serwisu urządzenia " + sg.serial_number + ". Zespół GEA Polska." + '\x001a');
                               //   Thread.Sleep(250);
                           }
                           if (rolaUzytkownika.ToString() == "admin")
                           {

                           }
                           if (rolaUzytkownika.ToString() == "gea")
                           {
                               port.WriteLine("Urządzenie " + sg.serial_number + " wymaga serwisu." + '\x001a');
                               //  Thread.Sleep(250);
                           }
                           //  Thread.Sleep(250);
                       }
                   }
                   if (dane[1] == "OK")  //jesteśmy w "zrób serwis", czyli serialnumber_OK
                   {
                       SerialPortController s = new SerialPortController();
                       var uzytkownik = bazaDanych.AspNetUsers.Find(s.User.Identity.GetUserId());
                       ServiceGuard SG = bazaDanych.ServiceGuard.Find(dane[0]);
                       var listaAlarmow = SG.Alarmy.ToList();
                       foreach (Alarmy a in listaAlarmow)
                       {
                           a.wykonal = uzytkownik.UserName;
                       }
                   }
                   if (dane[1] == "RESET")
                   {
                       ServiceGuard SG = bazaDanych.ServiceGuard.Find(dane[0]);
                       SG.ostatni_serwis = SG.ostatni_serwis_temp;
                       bazaDanych.SaveChangesAsync();
                   }
                   if (dane[1] == "C") //aktualizuj dane
                   {
                       string[] tmp = dane[5].Split("#".ToArray());//np. 0750#B
                       string przedzial = tmp[0]; //0750
                       string serialNumber = dane[0];
                       string aktualnyStanRoboczogodzin = dane[2];
                       string godzinyDoSerwisu = (Convert.ToInt16(przedzial) - Convert.ToInt16(dane[3])).ToString();
                       string lokalizacja = dane[4];
                       ServiceGuard SG = bazaDanych.ServiceGuard.Find(dane[0]);
                       SG.aktualny_czas = Convert.ToInt16(aktualnyStanRoboczogodzin);
                       SG.do_serwisu = Convert.ToInt32(godzinyDoSerwisu);
                       SG.lokalizacja = lokalizacja;
                       SG.przedzial_serwisowy = przedzial;
                       bazaDanych.SaveChanges();
                   }
                   if (dane[1] == "GSL") //zmień przedział serwisu
                   {
                       string serialNumber = dane[0];
                       ServiceGuard SG = bazaDanych.ServiceGuard.Find(dane[0]);
                       SG.przedzial_serwisowy = SG.przedzial_serwisowy_temp;
                       bazaDanych.SaveChanges();
                   }
                   if (dane[1] == "LNG") //zmień język
                   {
                       string serialNumber = dane[0];
                       ServiceGuard SG = bazaDanych.ServiceGuard.Find(dane[0]);
                       SG.jezyk = SG.jezyk_temp;
                       bazaDanych.SaveChanges();
                   }
                   if (dane[1] == "ZGS") //ustaw czas startowy
                   {
                       string serialNumber = dane[0];  //olac
                   }
                   if (dane[1] == "AGS") //ustaw czas total
                   {
                       string serialNumber = dane[0];
                       ServiceGuard SG = bazaDanych.ServiceGuard.Find(dane[0]);
                       SG.aktualny_czas = SG.aktualny_czas_temp;
                       bazaDanych.SaveChanges();
                   }
                   if (dane[1] == "SOK") //zmień serial number
                   {
                       string serialNumber = dane[0];
                       ServiceGuard SG = bazaDanych.ServiceGuard.Find(dane[0]);
                       SG.serial_number = SG.serial_number_temp;
                       bazaDanych.SaveChanges();
                   }
                   if (dane[1] == "VOK") //zmień super visor
                   {
                       string serialNumber = dane[0];
                       ServiceGuard SG = bazaDanych.ServiceGuard.Find(dane[0]);
                       SG.nrSuperVisor = SG.nrSuperVisor_temp;
                       bazaDanych.SaveChanges();
                   }
                   if (dane[1] == "NOK") //zmień service center
                   {
                       string serialNumber = dane[0];
                       ServiceGuard SG = bazaDanych.ServiceGuard.Find(dane[0]);
                       SG.nrServiceCenter = SG.nrServiceCenter_temp;
                       bazaDanych.SaveChanges();
                   }
                   if (dane[1] == "GOK") //zmień service manager
                   {
                       string serialNumber = dane[0];
                       ServiceGuard SG = bazaDanych.ServiceGuard.Find(dane[0]);
                       SG.ServiceManager.nr_tel = SG.ServiceManager.nr_tel_temp;
                       bazaDanych.SaveChanges();
                   }
               }
               catch (Exception ex)
               {
                   string s = ex.InnerException.ToString();
                   //nic nie rob, jesli szukamy za daleko w podzielonym stringu, to po prostu mamy krotsza odpowiedz
               }
           }
       }
        
        public void aktualizujDane(string nrServiceGuard)
        {    
           // UstawTrybWiadomosci();
            Entities baza = new Entities();
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
          //  var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " aktualizuje dane na SG o numerze " + nrServiceGuard + ".";
            nowyWpis.czas = DateTime.Now;
            baza.Logi.Add(nowyWpis);
            port.Write("AT+CMGF=1\r");
            Thread.Sleep(250);
            port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
            Thread.Sleep(100);
            port.WriteLine("F" + "\x11" + "C" + '\x001a');             
            Biling daneDoZapisu = new Biling();
            Guid s = Guid.NewGuid();
            daneDoZapisu.ServiceGuard_id_sg = nrServiceGuard;
            daneDoZapisu.tresc = "F_C";
            daneDoZapisu.Id_b = s.ToString();
            daneDoZapisu.data=DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;       
            baza.Biling.Add(daneDoZapisu);
            baza.SaveChanges();
        }    
        public void wyslijDowolnySMS(string nrTel,string tresc)
        {
         //   UstawTrybWiadomosci(); 
            string numerTel =nrTel;
            String komenda = "AT+CMGS=\"" + numerTel + "\"";
            string tekstDoWyslania =tresc;
            string tekstWyslany = string.Format("{0}{1}{2}{3}", komenda, enter, tekstDoWyslania, ctrlZ);
            port.WriteLine(tekstWyslany);
            Entities baza = new Entities();
            Biling daneDoZapisu = new Biling();
            Guid s = Guid.NewGuid();         
            daneDoZapisu.Id_b = s.ToString();
            daneDoZapisu.data = DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;
            daneDoZapisu.tresc = tekstWyslany;
            baza.Biling.Add(daneDoZapisu);
            baza.SaveChanges();
        }
        public void zrobSerwis(string nrServiceGuard)
        {
            Entities baza = new Entities();
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " robi serwis na SG o numerze " + nrServiceGuard + ".";
            nowyWpis.czas=DateTime.Now;
            baza.Logi.Add(nowyWpis);
            if (!port.IsOpen)
                port.Open();
            port.Write("AT+CMGF=1\r");
            Thread.Sleep(250);
            port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
            Thread.Sleep(100);
            port.WriteLine("F" + "\x11" + "RESET" +  '\x001a');
            Biling daneDoZapisu = new Biling();
            daneDoZapisu.data = DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;
            Guid s = Guid.NewGuid();
            daneDoZapisu.ServiceGuard_id_sg = nrServiceGuard;
            daneDoZapisu.Id_b = s.ToString();
            daneDoZapisu.tresc = "F_RESET";
            sg.ostatni_serwis_temp = DateTime.Now;
            baza.Biling.Add(daneDoZapisu);
            baza.SaveChanges();      
        }
        public void zmienPrzedzialSerwisu(string nrServiceGuard,string liczbaGodzin, string rodzajSerwisu)
        {
         //   UstawTrybWiadomosci();
            Entities baza = new Entities();
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " zmienia przedział serwisowy na SG o numerze " + nrServiceGuard + " na liczbe godzin " + liczbaGodzin + " i rodzaj serwisu " + rodzajSerwisu + ".";
            nowyWpis.czas=DateTime.Now;
            baza.Logi.Add(nowyWpis);
            string d = ViewBag.nrServiceGuard;
            if (!port.IsOpen)
                port.Open();
            port.Write("AT+CMGF=1\r");
            Thread.Sleep(250);
            port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
            Thread.Sleep(100);
            port.WriteLine("F" + "\x11" + "GS"+'\x11'+liczbaGodzin+"#"+rodzajSerwisu + '\x001a'); 
            Biling daneDoZapisu = new Biling();
            Guid s = Guid.NewGuid();
            daneDoZapisu.ServiceGuard_id_sg = nrServiceGuard;
            daneDoZapisu.Id_b = s.ToString();
            daneDoZapisu.data = DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;
            daneDoZapisu.tresc = "F_GS_" + liczbaGodzin + "#" + rodzajSerwisu;
            baza.Biling.Add(daneDoZapisu);
            sg.przedzial_serwisowy_temp = liczbaGodzin+rodzajSerwisu;
            baza.SaveChanges();
        }
        public void zmienJezyk(string nrServiceGuard,string kodJezyka)
        {
         //   UstawTrybWiadomosci();
            Entities baza = new Entities();
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " zmienia język na SG o numerze " + nrServiceGuard + " na " + kodJezyka + ".";
           nowyWpis.czas= DateTime.Now;
            baza.Logi.Add(nowyWpis);
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            if (!port.IsOpen)
                port.Open();
            port.Write("AT+CMGF=1\r");
            Thread.Sleep(250);
            port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
            Thread.Sleep(100);
            port.WriteLine("F" + "\x11" + "LN" + '\x11' + kodJezyka + '\x001a'); 
            Biling daneDoZapisu = new Biling();
            Guid s = Guid.NewGuid();
            daneDoZapisu.ServiceGuard_id_sg = nrServiceGuard;
            daneDoZapisu.Id_b = s.ToString();
            daneDoZapisu.data = DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;
            daneDoZapisu.tresc = "F_LN_"+kodJezyka;
            baza.Biling.Add(daneDoZapisu);
            sg.jezyk_temp = kodJezyka;
            baza.SaveChanges();
        }
        public void ustawCzasStartowy(string nrServiceGuard, string iloscGodzin)
        {
         //   UstawTrybWiadomosci();
            Entities baza = new Entities();
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " ustawia czas startowy na SG o numerze " + nrServiceGuard + " na " + iloscGodzin + ".";
            nowyWpis.czas = DateTime.Now;
            baza.Logi.Add(nowyWpis);
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            if (!port.IsOpen)
                port.Open();
            port.Write("AT+CMGF=1\r");
            Thread.Sleep(250);
            port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
            Thread.Sleep(100);
            port.WriteLine("F" + "\x11" + iloscGodzin + '\x001a'); 
            Biling daneDoZapisu = new Biling();
            Guid s = Guid.NewGuid();
            daneDoZapisu.ServiceGuard_id_sg = nrServiceGuard;
            daneDoZapisu.Id_b = s.ToString();
            daneDoZapisu.data = DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;
            daneDoZapisu.tresc = "F_"+iloscGodzin; //wiecej w bazie nie grzebie przez design - to jest ok
            baza.Biling.Add(daneDoZapisu);
            baza.SaveChanges();
        }
        public void ustawCzasTotal(string nrServiceGuard, string iloscGodzin)
        {
            Entities baza = new Entities();
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " ustawia czas total na SG o numerze " + nrServiceGuard + " na " + iloscGodzin + ".";
            nowyWpis.czas = DateTime.Now;
            baza.Logi.Add(nowyWpis);
            if (!port.IsOpen)
                port.Open();
            port.Write("AT+CMGF=1\r");
            Thread.Sleep(250);
            port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
            Thread.Sleep(100);
            port.WriteLine("F" + "\x11" + "GA" + "\x11" + iloscGodzin + '\x001a');
            Biling daneDoZapisu = new Biling();
            daneDoZapisu.data = DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;
            daneDoZapisu.tresc = "F_GA_"+iloscGodzin;
            sg.aktualny_czas_temp = Convert.ToInt16(iloscGodzin);
            baza.Biling.Add(daneDoZapisu);
            baza.SaveChanges();
        }
       
        public void zmienSerialNumber(string nrServiceGuard, string serialNumber)
        {
         //   UstawTrybWiadomosci();
              Entities baza = new Entities();
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " zmienia numer seryjny na SG o numerze " + nrServiceGuard + " na " + serialNumber + ".";
            nowyWpis.czas = DateTime.Now;
            baza.Logi.Add(nowyWpis);
            if (!port.IsOpen)
                port.Open();
            port.Write("AT+CMGF=1\r");
            Thread.Sleep(250);
            port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
            Thread.Sleep(100);
            port.WriteLine("F" + "\x11" + "S" + "\x11" + serialNumber + '\x001a');
            Biling daneDoZapisu = new Biling();
            daneDoZapisu.data = DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;
            Guid s = Guid.NewGuid();
            daneDoZapisu.ServiceGuard_id_sg = nrServiceGuard;
            daneDoZapisu.Id_b = s.ToString();
            daneDoZapisu.tresc = "F_S_"+serialNumber;
            baza.Biling.Add(daneDoZapisu);
            sg.serial_number_temp=serialNumber;
            baza.SaveChanges();
        }
        public void zmienNumerSuperVisor(string nrServiceGuard, string nrSuperVisor)
        {
        //    UstawTrybWiadomosci();       
            Entities baza = new Entities();
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " zmienia numer SuperVisor na SG o numerze " + nrServiceGuard + " na " + nrSuperVisor + ".";
            nowyWpis.czas = DateTime.Now;
            baza.Logi.Add(nowyWpis);
            if (!port.IsOpen)
                port.Open();
            port.Write("AT+CMGF=1\r");
            Thread.Sleep(250);
            port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
            Thread.Sleep(100);
            port.WriteLine("F" + "\x11" + "GV" + "\x11" + nrSuperVisor + '\x001a');
            Biling daneDoZapisu = new Biling();
            daneDoZapisu.data = DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;
            sg.nrSuperVisor_temp = nrSuperVisor;
            Guid s = Guid.NewGuid();
            daneDoZapisu.ServiceGuard_id_sg = nrServiceGuard;
            daneDoZapisu.Id_b = s.ToString();
            daneDoZapisu.tresc = "F_GV_"+nrSuperVisor;
            baza.Biling.Add(daneDoZapisu);
            baza.SaveChanges();
        }
        public void zmienNumerServiceCenter(string nrServiceGuard, string nrServiceCenter)
        {      
            Entities baza = new Entities();
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " zmienia numer ServiceCenter na SG o numerze " + nrServiceGuard + " na " + nrServiceCenter + ".";
            nowyWpis.czas = DateTime.Now;
            baza.Logi.Add(nowyWpis);
            if (!port.IsOpen)
                port.Open();
            port.Write("AT+CMGF=1\r");
            Thread.Sleep(250);
            port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
            Thread.Sleep(100);
            port.WriteLine("F" + "\x11" + "GN" + "\x11" + nrServiceCenter + '\x001a');
            Biling daneDoZapisu = new Biling();
            daneDoZapisu.data = DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;
            Guid s = Guid.NewGuid();
            daneDoZapisu.ServiceGuard_id_sg = nrServiceGuard;
            daneDoZapisu.Id_b = s.ToString();
            daneDoZapisu.tresc = "F_GN_" + nrServiceCenter;
            baza.Biling.Add(daneDoZapisu);
            sg.nrServiceCenter_temp = nrServiceCenter;
            baza.SaveChanges();
        }
        public void zmienNumerServiceManager(string nrServiceGuard, string nrServiceManager)
        {
            Entities baza = new Entities();
            ServiceGuard sg = baza.ServiceGuard.Find(nrServiceGuard);
            var uzytkownik = baza.AspNetUsers.Find(User.Identity.GetUserId());
            Logi nowyWpis = new Logi();
            nowyWpis.id_uzytkownik = User.Identity.GetUserId();
            nowyWpis.co_zrobil = "Dealer o loginie " + uzytkownik.UserName + " zmienia numer ServiceManager na SG o numerze " + nrServiceGuard + " na " + nrServiceManager + ".";
            nowyWpis.czas = DateTime.Now;
            baza.Logi.Add(nowyWpis);
            if (!port.IsOpen)
                port.Open();
            port.Write("AT+CMGF=1\r");
            Thread.Sleep(250);
            port.WriteLine("AT+CMGS=\"" + sg.nr_tel + "\"\r");
            Thread.Sleep(100);
            port.WriteLine("F" + "\x11" + "G" + "\x11" + nrServiceManager + '\x001a');
            Biling daneDoZapisu = new Biling();
            daneDoZapisu.data = DateTime.Now;
            daneDoZapisu.login = User.Identity.Name;
            Guid s = Guid.NewGuid();
            daneDoZapisu.ServiceGuard_id_sg = nrServiceGuard;
            daneDoZapisu.Id_b = s.ToString();
            daneDoZapisu.tresc = "F_G_" + nrServiceManager;
            baza.Biling.Add(daneDoZapisu);
            sg.ServiceManager.nr_tel_temp = nrServiceManager;
            baza.SaveChanges();     
        }
       
    }
}