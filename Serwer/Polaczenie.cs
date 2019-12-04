using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Serwer
{
    class Polaczenie
    {
        StringBuilder stringBuilder = new StringBuilder();

        private string id, daneHost;
        private int losKlienta, port;
        private bool koniecPolaczenia = false;

        public Polaczenie(string ip, int port, int id)
        {
            this.port = port;

            // Tworzenie 3-bitowego ID
            this.id = Convert.ToString(id, 2);

            stringBuilder.Append('0', 3 - this.id.Length);

            this.id = stringBuilder.ToString() + this.id;

            stringBuilder.Clear();

            UruchomSerwer(port, IPAddress.Parse(ip));
        }

        private void OdbierzPrzedzial(TcpClient client)
        {
            Stream stream = client.GetStream();

            byte[] temp = new byte[4];

            // Odebranie 4 bajtow
            stream.Read(temp, 0, temp.Length);

            // Zamiana bajtow na bity i polaczenie ich w jeden ciag
            string dane = ZamienNaBinarny(temp[0]) + ZamienNaBinarny(temp[1]) + ZamienNaBinarny(temp[2]) + ZamienNaBinarny(temp[3]);

            // Pozyskiwanie liczby L1 lub L2 w zaleznosci od wybranego portu 
            if (port == 13000)
            {
                Program.L1 = Convert.ToInt32(dane.Substring(12, 16), 2);
                if (Program.L1 > 32768)
                {
                    Program.L1 -= 65536;
                }
                Console.WriteLine($"Otrzymano L1 od ({daneHost}) = {Program.L1}");
                Program.L1Odebrana = true;
            }

            else
            {
                Program.L2 = Convert.ToInt32(dane.Substring(12, 16), 2);
                if (Program.L2 > 32768)
                {
                    Program.L2 -= 65536;
                }
                Console.WriteLine($"Otrzymano L2 od ({daneHost}) = {Program.L2}");
                Program.L2Odebrana = true;
            }

            if (Program.L1Odebrana && Program.L2Odebrana && Program.wylosowane == false) Program.Los(Program.L1, Program.L2);
        }

        private void WyslijPoczatekPrzedzialu(TcpClient client)
        {
            Stream stream = client.GetStream();

            string dane = string.Empty, L1String;

            //Ustalanie poczatku przedzialu (L1 - L2)
            Int16 wynik = (short)(Convert.ToInt16(Program.L1) - Convert.ToInt16(Program.L2));

            //Konwersja poczatku przedzialu na format binarny
            L1String = Convert.ToString(wynik, 2);

            // Dopisanie pola operacji
            dane += Program.ODP_SERWERA;

            // Dopisanie pola odpowiedzi
            dane += Program.ODP_POCZATEK;

            // Dopisanie ID
            dane += id;

            stringBuilder.Clear();

            // Dopasowanie liczby do 16 bitow
            stringBuilder.Append('0', 16 - L1String.Length);

            dane += stringBuilder.ToString() + L1String;

            // Dopisanie dopelnienia
            dane += "0000";

            // Zamiania bitow na bajty
            byte[] daneWys = new byte[] { (byte)Convert.ToInt32(dane.Substring(0, 8), 2), (byte)Convert.ToInt32(dane.Substring(8, 8), 2),
                (byte)Convert.ToInt32(dane.Substring(16, 8), 2), (byte)Convert.ToInt32(dane.Substring(24), 2) };

            // Wyslanie pakietu z danymi
            stream.Write(daneWys, 0, daneWys.Length);
        }

        private void WyslijKoniecPrzedzialu(TcpClient client)
        {
            Stream stream = client.GetStream();

            string dane = string.Empty, L2String;

            //Ustalanie poczatku przedzialu (L1 + L2)
            Int16 wynik = (short)(Convert.ToInt16(Program.L1) + Convert.ToInt16(Program.L2));

            //Konwersja poczatku przedzialu na format binarny
            L2String = Convert.ToString(wynik, 2);

            // Dopisanie pola operacji
            dane += Program.ODP_SERWERA;

            // Dopisanie pola odpowiedzi
            dane += Program.ODP_KONIEC;

            // Dopisanie ID
            dane += id;

            stringBuilder.Clear();

            // Dopasowanie liczby do 16 bitow
            stringBuilder.Append('0', 16 - L2String.Length);

            dane += stringBuilder.ToString() + L2String;

            // Dopisanie dopelnienia
            dane += "0000";

            // Zamiania bitow na bajty
            byte[] daneWys = new byte[] { (byte)Convert.ToInt32(dane.Substring(0, 8), 2), (byte)Convert.ToInt32(dane.Substring(8, 8), 2),
                (byte)Convert.ToInt32(dane.Substring(16, 8), 2), (byte)Convert.ToInt32(dane.Substring(24), 2) };

            // Wyslanie pakietu z danymi
            stream.Write(daneWys, 0, daneWys.Length);
        }

        private void OdbierzLiczbe(TcpClient client)
        {
            Stream stream = client.GetStream();

            byte[] temp = new byte[4];

            // Odebranie 4 bajtow
            stream.Read(temp, 0, temp.Length);

            // Zamiana bajtow na bity i polaczenie ich w jeden ciag
            string dane = ZamienNaBinarny(temp[0]) + ZamienNaBinarny(temp[1]) + ZamienNaBinarny(temp[2]) + ZamienNaBinarny(temp[3]);

            // Zamina liczby podanej przez klienta na format dziesietny
            losKlienta = Convert.ToInt16(dane.Substring(12, 16), 2);

            if (!Program.odgadnieta)
            {
                // Liczba zostala odgadnieta
                if (losKlienta == Program.tajnaLiczba)
                {
                    Console.WriteLine($"Liczba {Program.tajnaLiczba} zostala odgadnięta (Host: {daneHost})");

                    Program.odgadnieta = true;

                    // Czyszczenie bufora danych
                    dane = string.Empty;

                    // Dopisanie pola operacji
                    dane += Program.ODP_SERWERA;

                    // Dopisanie pola odpowiedzi
                    dane += Program.ODP_ZGADLES;

                    // Dopisanie ID
                    dane += id;

                    // Dopisanie pustej liczby oraz dopelnienia
                    dane += "00000000000000000000";

                    // Zamiania bitow na bajty
                    byte[] daneWys = new byte[] { (byte)Convert.ToInt32(dane.Substring(0, 8), 2), (byte)Convert.ToInt32(dane.Substring(8, 8), 2),
                                                  (byte)Convert.ToInt32(dane.Substring(16, 8), 2), (byte)Convert.ToInt32(dane.Substring(24), 2) };

                    // Wyslanie pakietu z danymi
                    stream.Write(daneWys, 0, daneWys.Length);
                }

                else
                {
                    Console.WriteLine($"Wylosowana liczba (Host: {daneHost}): {losKlienta} Oczekiwana: {Program.tajnaLiczba}");

                    // Czyszczenie bufora danych
                    dane = string.Empty;

                    // Dopisanie pola operacji
                    dane += Program.ODP_SERWERA;

                    // Dopisanie pola odpowiedzi
                    dane += Program.ODP_NIE_ZGADLES;

                    // Dopisanie ID
                    dane += id;

                    // Dopisanie pustej liczby oraz dopelnienia
                    dane += "00000000000000000000";

                    // Zamiania bitow na bajty
                    byte[] daneWys = new byte[] { (byte)Convert.ToInt32(dane.Substring(0, 8), 2), (byte)Convert.ToInt32(dane.Substring(8, 8), 2),
                                                  (byte)Convert.ToInt32(dane.Substring(16, 8), 2), (byte)Convert.ToInt32(dane.Substring(24), 2)};

                    // Wyslanie pakietu z danymi
                    stream.Write(daneWys, 0, daneWys.Length);
                }
            }
            else
            {
                Console.WriteLine($"Host: ({daneHost}) spóźnił się (liczba została odgadnięta)");

                // Czyszczenie bufora danych
                dane = string.Empty;

                // Dopisanie pola operacji
                dane += Program.ODP_SERWERA;

                // Dopisanie pola odpowiedzi
                dane += Program.ODP_DRUGI_KLIENT_ZGADL;

                // Dopisanie ID
                dane += id;

                Int16 wynik = (short)(Program.tajnaLiczba);

                // Zamiana wylosowanej liczby na format binarny
                string tajnaLicznaString = Convert.ToString(wynik, 2);

                stringBuilder.Clear();

                // Dopasowanie liczby do 16 bitow
                stringBuilder.Append('0', 16 - tajnaLicznaString.Length);

                // Dopisanie liczby oraz dopelnienia
                dane += stringBuilder.ToString() + tajnaLicznaString + "0000";

                // Zamiania bitow na bajty
                byte[] daneWys = new byte[] { (byte)Convert.ToInt32(dane.Substring(0, 8), 2), (byte)Convert.ToInt32(dane.Substring(8, 8), 2),
                                              (byte)Convert.ToInt32(dane.Substring(16, 8), 2), (byte)Convert.ToInt32(dane.Substring(24), 2) };

                // Wyslanie pakietu z danymi
                stream.Write(daneWys, 0, daneWys.Length);
            }
        }

        private void UruchomSerwer(int port, IPAddress localAddr)
        {
            TcpListener server = null;

            try
            {
                server = new TcpListener(localAddr, port);

                server.Start();

                byte[] bytes = new byte[2];

                while (!koniecPolaczenia)
                {
                    TcpClient client = server.AcceptTcpClient();

                    // Ilosc aktywnych polaczen
                    Program.polaczenia++;

                    // Dane klienta
                    daneHost = $"ID: {Convert.ToInt32(id, 2)}, IP: {((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}, Port: {port}";

                    Console.WriteLine($"Połączono ({daneHost})");

                    WyslijID(client);

                    if (!Program.L1Odebrana && !Program.L2Odebrana && Program.polaczenia == 2)
                    {
                        Console.WriteLine("Oczekiwanie na otzymanie przedziału od klientów");
                    }

                    OdbierzPrzedzial(client);

                    // Oczekiwanie na podanie L1 oraz L2
                    while (Program.L1Odebrana == false || Program.L2Odebrana == false) ;

                    WyslijPoczatekPrzedzialu(client);

                    WyslijKoniecPrzedzialu(client);

                    while (!Program.odgadnieta) { OdbierzLiczbe(client); }

                    // Jezeli wyloswana liczba zostala odgadnieta
                    if (Program.odgadnieta)
                    {
                        Stream stream = client.GetStream();

                        byte[] temp = new byte[4];

                        // Odebranie 4 bajtow
                        stream.Read(temp, 0, temp.Length);

                        // Sprawdzenie czy klient chce zakonczyc polaczenie
                        if (ZamienNaBinarny(temp[0]).Substring(0, 6) == Program.KONIEC_POLACZENIA)
                        {
                            Console.WriteLine($"Koniec połączenia (Host: {daneHost})");
                            koniecPolaczenia = true;
                            stream.Close();
                            client.Close();
                            server.Stop();
                        }
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine($"Wystąpił błąd, Treść błędu: {e.Message}");
            }
        }

        private void WyslijID(TcpClient klient)
        {
            Stream stream = klient.GetStream();

            // Czyszczenie bufora danych
            string dane = string.Empty;

            // Dopisanie pola operacji
            dane += Program.WYSLANIE_ID;

            // Dopisanie pola odpowiedzi
            dane += "000";

            // Dopisanie ID
            dane += id;

            // Dopisanie pustej liczby oraz dopelnienia
            dane += "00000000000000000000";

            // Zamiania bitow na bajty
            byte[] daneWys = new byte[] { (byte)Convert.ToInt32(dane.Substring(0, 8), 2), (byte)Convert.ToInt32(dane.Substring(8, 8), 2),
                                          (byte)Convert.ToInt32(dane.Substring(16, 8), 2), (byte)Convert.ToInt32(dane.Substring(24), 2) };

            // Wyslanie pakietu z danymi
            stream.Write(daneWys, 0, daneWys.Length);
        }

        public static string ZamienNaBinarny(int liczba)
        {
            string temp = string.Empty;

            while (liczba / 2 > 0)
            {
                if (liczba % 2 == 0)
                {
                    temp += "0";
                }

                else
                {
                    temp += "1";
                }

                liczba /= 2;
            }

            if (liczba % 2 == 0)
            {
                temp += "0";
            }

            else
            {
                temp += "1";
            }

            char[] charArray = temp.ToCharArray();
            Array.Reverse(charArray);

            temp = new string(charArray);

            int tempLiczba = 8 - temp.Length;
            string wynik = string.Empty;

            for (int i = 0; i < tempLiczba; i++)
            {
                wynik += "0";
            }

            wynik += temp;

            return wynik;
        }
    }
}
