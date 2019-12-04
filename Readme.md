Projekt na pierwszą część zajęć laboratoryjnych z przedmiotu Technologie Sieciowe.

Wariant 13 – model komunikacji 2 ↔ 1
•	Protokół warstwy transportowej: TCP.
•	Struktura nagłówka protokołu binarnego: pole operacji (6 bitów), pole odpowiedzi (3 bity), pole identyfikatora sesji (3 bity), dodatkowe pola zdefiniowane przez programistę.
•	Funkcje oprogramowania:
o	Klienta:
    - Nawiązanie połączenia z serwerem,
    - Uzyskanie identyfikatora sesji,
    - Przesłanie pojedynczej liczby L,
    - Przesyłanie wartości liczbowych, będących „odpowiedziami”:
    - Klient ma odgadnąć liczbę wylosowaną przez serwer;
    - Zakończenie połączenia;
o	Serwera:
    - Wygenerowanie identyfikatora sesji,
    - Wylosowanie liczby tajnej z przedziału (L1 – L2; L1 + L2),
    - Przesłanie przedziału, w którym zawiera się liczba do zgadnięcia,
    - Informowanie klientów, czy wartość została odgadnięta.
•	Wymagania dodatkowe: 
    - Identyfikator sesji powinien być przesyłany w każdym komunikacie 
