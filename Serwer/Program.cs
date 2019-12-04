using System;
using System.Threading;

namespace Serwer
{
    class Program
    {
        // Pola operacji     
        public static readonly string WYSLANIE_ID = "000010";
        public static readonly string LString = "001000";
        public static readonly string ODP_SERWERA = "100000";
        public static readonly string KONIEC_POLACZENIA = "100010";

        // Pola odpowiedzi
        public static readonly string ODP_ZGADLES = "100";
        public static readonly string ODP_NIE_ZGADLES = "101";
        public static readonly string ODP_POCZATEK = "001";
        public static readonly string ODP_KONIEC = "010";
        public static readonly string ODP_DRUGI_KLIENT_ZGADL = "111";

        private static Random random = new Random();

        private static string ip = "192.168.1.100";

        public static int ID = 1, port = 13000, L1, L2, tajnaLiczba, polaczenia = 0;

        public static bool odgadnieta = false, L1Odebrana = false, L2Odebrana = false, wylosowane = false;

        static void Main(string[] args)
        {
            // Tworzenie watkow dla klientow
            new Thread(() => { new Polaczenie(ip, port++, ID++); }).Start();

            Console.Write($"Uruchomino serwer o adresie IP: {ip} na portach {port}, ");

            new Thread(() => { new Polaczenie(ip, port++, ID++); }).Start();

            Console.Write($"{port} ");

            Console.WriteLine("Oczekiwanie na połączenie klientów");

            Console.ReadKey();
        }

        public static void Los(int L1, int L2) // Losowanie tajnej liczby
        {
            Console.WriteLine($"Przedział: {L1 - L2} do {L1 + L2}");
            tajnaLiczba = random.Next(L1 - L2, L1 + L2);
            wylosowane = true;
            Console.WriteLine($"Tajna liczba wynosi: {tajnaLiczba}");
        }
    }
}